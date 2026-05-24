#define FenGen_ParseKeywordDuplicateDest

using System.Runtime.CompilerServices;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    // Generated version that doesn't do manual bounds checking, for when we know we're far enough from the end of the buffer
    private RtfError ParseKeyword_Fast(ref byte bufferRef, ref byte keywordRef)
    {
        bool hasParam = false;
        int param = 0;
        Symbol? symbol;

        int startingCurrentPos = _currentPos;

        // [FenGen:ScalarKeywordParseSection:Fast:Dest:Begin]
        char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);

        if (!CharExtension.IsAsciiLetter(ch))
        {
            /*
            From the spec:
            "A control symbol consists of a backslash followed by a single, non-alphabetical character.
            For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
            delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."
            */

            // Fast path for destination marker - claws us back a small amount of perf
            if (ch == '*')
            {
                _skipDestinationIfUnknown = true;
                return RtfError.OK;
            }

            symbol = LookUpControlSymbol((byte)ch);

            if (symbol == null)
            {
                if (_skipDestinationIfUnknown)
                {
                    SkipDest(ref bufferRef);
                }
                _skipDestinationIfUnknown = false;
                return RtfError.OK;
            }

            _skipDestinationIfUnknown = false;

            return DispatchKeyword(ref bufferRef, ref keywordRef, symbol, param, hasParam);
        }
        else
        {
            byte keywordCount;
            for (keywordCount = 0;
                 keywordCount < _keywordMaxLen + 1 && CharExtension.IsAsciiLetter(ch);
                 keywordCount++, ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef))
            {
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref keywordRef, (nint)keywordCount), (byte)ch);
            }
            if (keywordCount > _keywordMaxLen)
            {
                return RtfError.KeywordTooLong;
            }

            int negateParam = 0;
            if (ch == '-')
            {
                negateParam = 1;
                ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
            }
            if (CharExtension.IsAsciiDigit(ch))
            {
                hasParam = true;
                checked
                {
                    try
                    {
                        int i;
                        for (i = 0;
                             i < _paramMaxLen + 1 && CharExtension.IsAsciiDigit(ch);
                             i++, ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef))
                        {
                            param = (param * 10) + (ch - '0');
                        }
                        if (i > _paramMaxLen)
                        {
                            return RtfError.ParameterOutOfRange;
                        }
                    }
                    catch (OverflowException)
                    {
                        return RtfError.ParameterOutOfRange;
                    }
                }
                /*
                NOTE: Turns out the branches are actually faster than the branchless black magic. On all targets.
                Go figure...
                */
                // This negate is safe, because int max negated is -2147483647, and int min is -2147483648
                if (negateParam == 1) param = -param;
            }

            if (ch != ' ') --_currentPos;
            // [FenGen:ScalarKeywordParseSection:Fast:Dest:End]

            ref byte keywordRefLocal = ref GetRefAtPos(ref bufferRef, startingCurrentPos);

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1 && keywordRefLocal == (byte)'f')
            {
                symbol = _fontSymbol;
                _skipDestinationIfUnknown = false;
                return DispatchKeyword(ref bufferRef, ref keywordRef, symbol, param, hasParam);
            }
            else
            {
                symbol = LookUpControlWord(ref keywordRef, keywordCount);
            }

            if (symbol == null)
            {
                if (_skipDestinationIfUnknown)
                {
                    SkipDest(ref bufferRef);
                }
                _skipDestinationIfUnknown = false;
                return RtfError.OK;
            }

            _skipDestinationIfUnknown = false;

            return DispatchKeyword(ref bufferRef, ref keywordRef, symbol, param, hasParam);
        }
    }
}
