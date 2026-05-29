#define FenGen_ParseKeywordDuplicateDest

using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    // Generated version that doesn't do manual bounds checking, for when we know we're far enough from the end of the buffer
    private RtfError ParseKeyword_Fast(ref byte bufferRef)
    {
        bool hasParam = false;
        int param = 0;

        int startingCurrentPos = _currentPos;

        // [FenGen:ScalarKeywordParseSection:Fast:Dest:Begin]
        char ch = (char)GetByteAtPos(ref bufferRef, startingCurrentPos);

        if (!CharExtension.IsAsciiLetter(ch))
        {
            ++_currentPos;

            return HandleControlChar(ref bufferRef, ch);
        }
        else
        {
            Symbol? symbol;
            byte keywordCount;
            for (keywordCount = 0;
                 keywordCount < _keywordMaxLen + 1 && CharExtension.IsAsciiLetter(ch);
                 keywordCount++,
                 ch = (char)GetByteAtPos(ref bufferRef, startingCurrentPos + keywordCount))
            {
            }
            if (keywordCount > _keywordMaxLen)
            {
                return RtfError.KeywordTooLong;
            }

            int accumulatedPos = startingCurrentPos + keywordCount;
            int negateParam = 0;
            if (ch == '-')
            {
                negateParam = 1;
                accumulatedPos += 1;
                ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos);
            }
            if (CharExtension.IsAsciiDigit(ch))
            {
                hasParam = true;
                checked
                {
                    try
                    {
                        int paramLength;
                        for (paramLength = 0;
                             paramLength < _paramMaxLen + 1 && CharExtension.IsAsciiDigit(ch);
                             paramLength++,
                             ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + paramLength))
                        {
                            param = (param * 10) + (ch - '0');
                        }
                        if (paramLength > _paramMaxLen)
                        {
                            return RtfError.ParameterOutOfRange;
                        }
                        accumulatedPos += paramLength;
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

            _currentPos = accumulatedPos + (ch == ' ' ? 1 : 0);
            // [FenGen:ScalarKeywordParseSection:Fast:Dest:End]

            ref byte keywordRef = ref GetRefAtPos(ref bufferRef, startingCurrentPos);

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1 && keywordRef == (byte)'f')
            {
                symbol = _fontSymbol;
                _skipDestinationIfUnknown = false;
                return DispatchKeyword(ref bufferRef, symbol, param, hasParam);
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

            return DispatchKeyword(ref bufferRef, symbol, param, hasParam);
        }
    }
}
