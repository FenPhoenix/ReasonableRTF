#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private readonly Vector512<byte>[] _symbolFontNameVectors512 = new Vector512<byte>[_symbolArraysLength];
    private readonly Vector256<byte>[] _symbolFontNameVectors256 = new Vector256<byte>[_symbolArraysLength];
    private readonly Vector128<byte>[] _symbolFontNameVectors128 = new Vector128<byte>[_symbolArraysLength];

    private void InitSymbolFontNameVectors()
    {
        Span<byte> bytes512 = stackalloc byte[Vector512<byte>.Count];
        Span<byte> bytes256 = stackalloc byte[Vector256<byte>.Count];
        Span<byte> bytes128 = stackalloc byte[Vector128<byte>.Count];

        for (int i = _symbolArraysStartingIndex; i < _symbolArraysLength; i++)
        {
            _symbolFontNameVectors512[i] = GetZeroPaddedVector512(bytes512, _symbolFontCharsArrays[i]);
            _symbolFontNameVectors256[i] = GetZeroPaddedVector256(bytes256, _symbolFontCharsArrays[i]);
            _symbolFontNameVectors128[i] = GetZeroPaddedVector128(bytes128, _symbolFontCharsArrays[i]);
        }

        return;

        static Vector512<byte> GetZeroPaddedVector512(Span<byte> bytes, byte[] name)
        {
            if (name.Length > Vector512<byte>.Count)
            {
                return Vector512<byte>.Zero;
            }

            bytes.Clear();
            name.CopyTo(bytes);

            return Vector512.Create(bytes);
        }

        static Vector256<byte> GetZeroPaddedVector256(Span<byte> bytes, byte[] name)
        {
            if (name.Length > Vector256<byte>.Count)
            {
                return Vector256<byte>.Zero;
            }

            bytes.Clear();
            name.CopyTo(bytes);

            return Vector256.Create(bytes);
        }

        static Vector128<byte> GetZeroPaddedVector128(Span<byte> bytes, byte[] name)
        {
            if (name.Length > Vector128<byte>.Count)
            {
                return Vector128<byte>.Zero;
            }

            bytes.Clear();
            name.CopyTo(bytes);

            return Vector128.Create(bytes);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SymbolFont SIMD_TryGetFontName(ref byte bufferRef, char ch)
    {
        if (Vector512.IsHardwareAccelerated && _currentPos < _currentBufferChunkLength - (Vector512<byte>.Count + 1))
        {
            _currentPos--;

            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, _currentPos);
            Vector512<byte> vector = Vector512.LoadUnsafe(ref searchSpace);
            Vector512<byte> equalsTerminatingChar =
                Vector512.Equals(_zeroVector512, vector) |
                Vector512.Equals(_lfVector512, vector) |
                Vector512.Equals(_crVector512, vector) |
                Vector512.Equals(_backslashVector512, vector) |
                Vector512.Equals(_openBraceVector512, vector) |
                Vector512.Equals(_closingBraceVector512, vector) |
                Vector512.Equals(_semicolonVector512, vector);

            if (equalsTerminatingChar != Vector512<byte>.Zero)
            {
                int terminatingCharIndex = BitOperations.TrailingZeroCount(equalsTerminatingChar.ExtractMostSignificantBits());
                ch = (char)Unsafe.AddByteOffset(ref searchSpace, (nint)terminatingCharIndex);

                if (EarlyOut(terminatingCharIndex))
                {
                    _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                    return SymbolFont.None;
                }

                Vector512<byte> maskVec = Vector512.GreaterThan(Vector512.Create((byte)terminatingCharIndex), _indexVec_512);
                Vector512<byte> fontName = Vector512.BitwiseAnd(vector, maskVec);

                _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                return TryFindSymbolFont512(fontName, _symbolFontNameVectors512);
            }
            else
            {
                ch = (char)GetByteAtPos(ref bufferRef, _currentPos + Vector512<byte>.Count);
                if (ch == ';' || _isNonPlainText[(byte)ch])
                {
                    if (EarlyOut(Vector512<byte>.Count))
                    {
                        _currentPos += ch == ';' ? Vector512<byte>.Count + 1 : Vector512<byte>.Count;
                        return SymbolFont.None;
                    }

                    _currentPos += ch == ';' ? Vector512<byte>.Count + 1 : Vector512<byte>.Count;
                    return TryFindSymbolFont512(vector, _symbolFontNameVectors512);
                }
                else
                {
                    _currentPos += Vector512<byte>.Count;
                    if (Vector512<byte>.Count < _maxSupportedSymbolFontNameLength)
                    {
                        vector.CopyTo(_symbolFontNameBuffer);
                        return GetSymbolFont_Scalar(ref bufferRef, ch, Vector512<byte>.Count);
                    }
                    else
                    {
                        return SymbolFont.None;
                    }
                }
            }
        }
        else if (Vector256.IsHardwareAccelerated && _currentPos < _currentBufferChunkLength - (Vector256<byte>.Count + 1))
        {
            _currentPos--;

            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, _currentPos);
            Vector256<byte> vector = Vector256.LoadUnsafe(ref searchSpace);
            Vector256<byte> equalsTerminatingChar =
                Vector256.Equals(_zeroVector256, vector) |
                Vector256.Equals(_lfVector256, vector) |
                Vector256.Equals(_crVector256, vector) |
                Vector256.Equals(_backslashVector256, vector) |
                Vector256.Equals(_openBraceVector256, vector) |
                Vector256.Equals(_closingBraceVector256, vector) |
                Vector256.Equals(_semicolonVector256, vector);

            if (equalsTerminatingChar != Vector256<byte>.Zero)
            {
                int terminatingCharIndex = BitOperations.TrailingZeroCount(equalsTerminatingChar.ExtractMostSignificantBits());
                ch = (char)Unsafe.AddByteOffset(ref searchSpace, (nint)terminatingCharIndex);

                if (EarlyOut(terminatingCharIndex))
                {
                    _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                    return SymbolFont.None;
                }

                Vector256<byte> maskVec = Vector256.GreaterThan(Vector256.Create((byte)terminatingCharIndex), _indexVec_256);
                Vector256<byte> fontName = Vector256.BitwiseAnd(vector, maskVec);

                _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                return TryFindSymbolFont256(fontName, _symbolFontNameVectors256);
            }
            else
            {
                ch = (char)GetByteAtPos(ref bufferRef, _currentPos + Vector256<byte>.Count);
                if (ch == ';' || _isNonPlainText[(byte)ch])
                {
                    if (EarlyOut(Vector256<byte>.Count))
                    {
                        _currentPos += ch == ';' ? Vector256<byte>.Count + 1 : Vector256<byte>.Count;
                        return SymbolFont.None;
                    }

                    _currentPos += ch == ';' ? Vector256<byte>.Count + 1 : Vector256<byte>.Count;
                    return TryFindSymbolFont256(vector, _symbolFontNameVectors256);
                }
                else
                {
                    _currentPos += Vector256<byte>.Count;
                    if (Vector256<byte>.Count < _maxSupportedSymbolFontNameLength)
                    {
                        vector.CopyTo(_symbolFontNameBuffer);
                        return GetSymbolFont_Scalar(ref bufferRef, ch, Vector256<byte>.Count);
                    }
                    else
                    {
                        return SymbolFont.None;
                    }
                }
            }
        }
        else if (Vector128.IsHardwareAccelerated && _currentPos < _currentBufferChunkLength - (Vector128<byte>.Count + 1))
        {
            _currentPos--;

            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, _currentPos);
            Vector128<byte> vector = Vector128.LoadUnsafe(ref searchSpace);
            Vector128<byte> equalsTerminatingChar =
                Vector128.Equals(_zeroVector128, vector) |
                Vector128.Equals(_lfVector128, vector) |
                Vector128.Equals(_crVector128, vector) |
                Vector128.Equals(_backslashVector128, vector) |
                Vector128.Equals(_openBraceVector128, vector) |
                Vector128.Equals(_closingBraceVector128, vector) |
                Vector128.Equals(_semicolonVector128, vector);

            if (equalsTerminatingChar != Vector128<byte>.Zero)
            {
                int terminatingCharIndex = BitOperations.TrailingZeroCount(equalsTerminatingChar.ExtractMostSignificantBits());
                ch = (char)Unsafe.AddByteOffset(ref searchSpace, (nint)terminatingCharIndex);

                if (EarlyOut(terminatingCharIndex))
                {
                    _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                    return SymbolFont.None;
                }

                Vector128<byte> maskVec = Vector128.GreaterThan(Vector128.Create((byte)terminatingCharIndex), _indexVec_128);
                Vector128<byte> fontName = Vector128.BitwiseAnd(vector, maskVec);

                _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                return TryFindSymbolFont128(fontName, _symbolFontNameVectors128);
            }
            else
            {
                ch = (char)GetByteAtPos(ref bufferRef, _currentPos + Vector128<byte>.Count);
                if (ch == ';' || _isNonPlainText[(byte)ch])
                {
                    if (EarlyOut(Vector128<byte>.Count))
                    {
                        _currentPos += ch == ';' ? Vector128<byte>.Count + 1 : Vector128<byte>.Count;
                        return SymbolFont.None;
                    }

                    _currentPos += ch == ';' ? Vector128<byte>.Count + 1 : Vector128<byte>.Count;
                    return TryFindSymbolFont128(vector, _symbolFontNameVectors128);
                }
                else
                {
                    _currentPos += Vector128<byte>.Count;
                    if (Vector128<byte>.Count < _maxSupportedSymbolFontNameLength)
                    {
                        vector.CopyTo(_symbolFontNameBuffer);
                        return GetSymbolFont_Scalar(ref bufferRef, ch, Vector128<byte>.Count);
                    }
                    else
                    {
                        return SymbolFont.None;
                    }
                }
            }
        }
        else
        {
            return GetSymbolFont_Scalar(ref bufferRef, ch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool EarlyOut(int index)
        {
            return !index.IsBetween(_minSupportedSymbolFontNameLength, _maxSupportedSymbolFontNameLength) ||
                   !_symbolFontNameLengths[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static SymbolFont TryFindSymbolFont512(Vector512<byte> fontName, Vector512<byte>[] symbolFontNameVectors)
        {
            for (int i = _symbolArraysStartingIndex; i < _symbolArraysLength; i++)
            {
                if (fontName == symbolFontNameVectors[i])
                {
                    return (SymbolFont)i;
                }
            }

            return SymbolFont.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static SymbolFont TryFindSymbolFont256(Vector256<byte> fontName, Vector256<byte>[] symbolFontNameVectors)
        {
            for (int i = _symbolArraysStartingIndex; i < _symbolArraysLength; i++)
            {
                if (fontName == symbolFontNameVectors[i])
                {
                    return (SymbolFont)i;
                }
            }

            return SymbolFont.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static SymbolFont TryFindSymbolFont128(Vector128<byte> fontName, Vector128<byte>[] symbolFontNameVectors)
        {
            for (int i = _symbolArraysStartingIndex; i < _symbolArraysLength; i++)
            {
                if (fontName == symbolFontNameVectors[i])
                {
                    return (SymbolFont)i;
                }
            }

            return SymbolFont.None;
        }
    }
}
#endif
