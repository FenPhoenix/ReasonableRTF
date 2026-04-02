#nullable enable // Required for generated files
#define FenGen_ParseKeywordDuplicateDest

using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    // Generated version that doesn't do manual bounds checking, for when we know we're far enough from the end of the buffer
    private RtfError ParseKeyword_Fast()
    {
        bool hasParam = false;
        int param = 0;
        Symbol? symbol;

        char ch = (char)_buffer[IncrementCurrentPos()];

        byte[] keyword = _keyword;

        if (!CharExtension.IsAsciiLetter(ch))
        {
            /*
            From the spec:
            "A control symbol consists of a backslash followed by a single, non-alphabetical character.
            For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
            delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."

            So just go straight to dispatching without looking for a param and without eating the space.
            */

            // Fast path for destination marker - claws us back a small amount of perf
            if (ch == '*')
            {
                _skipDestinationIfUnknown = true;
                return RtfError.OK;
            }

            symbol = LookUpControlSymbol((byte)ch);
        }
        else
        {
            byte keywordCount;
            for (keywordCount = 0;
                 keywordCount < _keywordMaxLen + 1 && CharExtension.IsAsciiLetter(ch);
                 keywordCount++, ch = (char)_buffer[IncrementCurrentPos()])
            {
                keyword[keywordCount] = (byte)ch;
            }
            if (keywordCount > _keywordMaxLen)
            {
                return RtfError.KeywordTooLong;
            }

            int negateParam = 0;
            if (ch == '-')
            {
                negateParam = 1;
                ch = (char)_buffer[IncrementCurrentPos()];
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
                             i++, ch = (char)_buffer[IncrementCurrentPos()])
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
                // This negate is safe, because int max negated is -2147483647, and int min is -2147483648
                param = BranchlessConditionalNegate(param, negateParam);
            }

            /*
            From the spec:
            "As with all RTF keywords, a keyword-terminating space may be present (before the ANSI characters)
            that is not counted in the characters to skip."
            This implements the spec for regular control words and \uN alike. Nothing extra needed for removing
            the space from the skip-chars to count.
            */
            // Current position will be > 0 at this point, so a decrement is always safe
            _currentPos += MinusOneIfNotSpace_8Bits(ch);

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1 && keyword[0] == (byte)'f')
            {
                symbol = _fontSymbol;
                _skipDestinationIfUnknown = false;
                return DispatchKeyword(symbol, param, hasParam);
            }
            else
            {
                symbol = LookUpControlWord(keyword, keywordCount);
            }
        }

        if (symbol == null)
        {
            // If this is a new destination
            if (_skipDestinationIfUnknown)
            {
                SkipDest();
            }
            _skipDestinationIfUnknown = false;
            return RtfError.OK;
        }

        _skipDestinationIfUnknown = false;

        return DispatchKeyword(symbol, param, hasParam);
    }
}
