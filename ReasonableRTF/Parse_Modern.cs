#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReasonableRTF.Enums;
using ReasonableRTF.Models.Fonts;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private RtfError ParseRtf()
    {
        // Avoid bounds checks by passing a buffer reference everywhere. We do our own bounds checking.
        ref byte bufferRef = ref MemoryMarshal.GetArrayDataReference(_buffer);
        ref bool isNonPlainTextCharRef = ref MemoryMarshal.GetReference(_isNonPlainText);
        ref bool isIgnoreCharRef = ref MemoryMarshal.GetReference(_isIgnoreChar);

        int currentPosLocal = _currentPos;

        while (!_reachedEndOfStream)
        {
            while (currentPosLocal < _currentBufferChunkLength)
            {
                char ch = (char)Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);

                // Ordered by most frequently appearing first
                switch (ch)
                {
                    case '\\':
                        _currentPos = currentPosLocal;
                        RtfError ec = ParseKeyword(ref bufferRef);
                        currentPosLocal = _currentPos;
                        if (ec != RtfError.OK) return ec;
                        break;
                    case '{':
                        GroupStack_DeepCopyToNext();
                        break;
                    case '}':
                        if (_groupStackTopIndex == 0)
                        {
                            _currentPos = currentPosLocal;
                            return RtfError.StackUnderflow;
                        }
                        --_groupStackTopIndex;
                        if (_groupStackTopIndex == 0)
                        {
                            _currentPos = currentPosLocal;
                            return RtfError.OK;
                        }
                        break;
                    default:
                    {
                        if (!Unsafe.AddByteOffset(ref isIgnoreCharRef, (nint)ch) &&
                            !GroupStack_CurrentSkipDest &&
                            !GroupStack_CurrentPropertyHidden)
                        {
                            // No measurable perf loss from this, and it lets us avoid duplicating the loop body.
                            char currentChar;
                            if (currentPosLocal < _currentBufferChunkLength)
                            {
                                currentChar = (char)GetByteAtPos(ref bufferRef, currentPosLocal);
                            }
                            else
                            {
                                _currentPos = currentPosLocal;
                                currentChar = (char)GetByte(currentPosLocal);
                                currentPosLocal = _currentPos;
                            }

                            if (Unsafe.AddByteOffset(ref isNonPlainTextCharRef, (nint)currentChar))
                            {
                                SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
                                if (symbolFont > SymbolFont.Unset)
                                {
                                    AddCharFromConversionList((byte)ch, _symbolFontTables[(int)symbolFont]);
                                }
                                else
                                {
                                    PlainText_Add(ch);
                                }
                            }
                            else
                            {
                                _currentPos = currentPosLocal;
                                HandlePlainTextRun(ref bufferRef);
                                currentPosLocal = _currentPos;
                            }
                        }
                        break;
                    }
                }
            }

            if (_bufferedStream != null)
            {
                _currentPos = currentPosLocal;
                HandleOutOfBounds();
            }
            else
            {
                break;
            }

            currentPosLocal = _currentPos;
        }

        return _groupStackTopIndex > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    private void HandlePlainTextRun(ref byte bufferRef)
    {
        int currentPosLocal = _currentPos - 1;

        SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
        if (symbolFont > SymbolFont.Unset)
        {
            uint[] table = _symbolFontTables[(int)symbolFont];
            while (!_reachedEndOfStream)
            {
                while (currentPosLocal < _currentBufferChunkLength)
                {
                    char ch = (char)Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
                    if (!_isNonPlainText[(byte)ch])
                    {
                        AddCharFromConversionList((byte)ch, table);
                    }
                    else
                    {
                        _currentPos = currentPosLocal - 1;
                        return;
                    }
                }

                if (_bufferedStream != null)
                {
                    _currentPos = currentPosLocal;
                    HandleOutOfBounds();
                }
                else
                {
                    break;
                }

                currentPosLocal = _currentPos;
            }
        }
        else
        {
            if (System.Numerics.Vector.IsHardwareAccelerated)
            {
                bool finishedOnNonPlainTextChar = SIMD_CopyPlainText(ref bufferRef, currentPosLocal, out currentPosLocal);

                if (finishedOnNonPlainTextChar)
                {
                    _currentPos = currentPosLocal;
                    return;
                }
            }

            if (currentPosLocal < (_currentBufferChunkLength - 1) - _plainTextRunFastPathAmountBackFromBufferEnd &&
                _plainText_Count < (_plainText_Capacity - _plainTextRunFastPathAmountBackFromBufferEnd) - 1)
            {
                char[] plainText = _plainText;
                for (int i = 0; i < _plainTextRunFastPathAmountBackFromBufferEnd; i++)
                {
                    char ch = (char)Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
                    if (!_isNonPlainText[(byte)ch])
                    {
                        plainText[_plainText_Count++] = ch;
                    }
                    else
                    {
                        _currentPos = currentPosLocal - 1;
                        return;
                    }
                }
            }

            if (System.Numerics.Vector.IsHardwareAccelerated)
            {
                // Break out of the scalar loop at the buffer boundary, so that if the plaintext run continues
                // after the next buffer load, we'll be able to jump back into a SIMD parse.
                while (currentPosLocal < _currentBufferChunkLength)
                {
                    char ch = (char)Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
                    if (!_isNonPlainText[(byte)ch])
                    {
                        PlainText_Add(ch);
                    }
                    else
                    {
                        _currentPos = currentPosLocal - 1;
                        return;
                    }
                }
            }
            else
            {
                while (!_reachedEndOfStream)
                {
                    while (currentPosLocal < _currentBufferChunkLength)
                    {
                        char ch = (char)Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
                        if (!_isNonPlainText[(byte)ch])
                        {
                            PlainText_Add(ch);
                        }
                        else
                        {
                            _currentPos = currentPosLocal - 1;
                            return;
                        }
                    }

                    if (_bufferedStream != null)
                    {
                        _currentPos = currentPosLocal;
                        HandleOutOfBounds();
                    }
                    else
                    {
                        break;
                    }

                    currentPosLocal = _currentPos;
                }
            }
        }

        _currentPos = currentPosLocal;
    }

    private void HandleHexRun(ref byte bufferRef)
    {
        _hexBuffer_Count = 0;

        (ushort codePage, FontEntry fontEntry) = GetCurrentCodePage();

        byte byte1;
        byte byte2;

        int currentPosLocal = _currentPos;

        if (currentPosLocal < _currentBufferChunkLength - 1)
        {
            byte1 = Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
            byte2 = Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
        }
        else
        {
            byte1 = GetByte(IncrementCurrentPos());
            byte2 = GetByte(IncrementCurrentPos());
            currentPosLocal = _currentPos;
        }

        AddByteToHexBuffer(byte1, byte2);

        // TODO: Manually duplicated code for performance - should be automated if possible
        while (currentPosLocal < _currentBufferChunkLength - 3)
        {
            byte b = Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
            if (b == (byte)'\\')
            {
                b = Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
                if (b == (byte)'\'')
                {
                    byte1 = Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
                    byte2 = Unsafe.AddByteOffset(ref bufferRef, (nint)currentPosLocal++);
                    AddByteToHexBuffer(byte1, byte2);
                }
                else
                {
                    _currentPos = currentPosLocal - 2;
                    AddHexBuffer(codePage, in fontEntry);
                    return;
                }
            }
            // Spaces end a hex run, but linebreaks don't.
            else if (b is not (byte)'\r' and not (byte)'\n')
            {
                _currentPos = currentPosLocal - 1;
                AddHexBuffer(codePage, in fontEntry);
                return;
            }

            _currentPos = currentPosLocal;
        }

        _currentPos = currentPosLocal;

        while (!_reachedEndOfStream)
        {
            byte b = GetByte(IncrementCurrentPos());
            if (b == (byte)'\\')
            {
                b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\'')
                {
                    byte1 = GetByte(IncrementCurrentPos());
                    byte2 = GetByte(IncrementCurrentPos());
                    AddByteToHexBuffer(byte1, byte2);
                }
                else
                {
                    _currentPos -= 2;
                    AddHexBuffer(codePage, in fontEntry);
                    return;
                }
            }
            // Spaces end a hex run, but linebreaks don't.
            else if (b is not (byte)'\r' and not (byte)'\n')
            {
                _currentPos--;
                AddHexBuffer(codePage, in fontEntry);
                return;
            }
        }
    }
}
#endif
