#define FenGen_ParseKeywordDuplicateDest

using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private RtfError ParseKeyword_FontTable_Fast(out KeywordType fontTableKeyword, out int param)
    {
        bool hasParam = false;
        param = 0;
        Symbol? symbol;
        fontTableKeyword = default;

        // [FenGen:ScalarKeywordParseSection:Fast:Dest:Begin]
        char ch = (char)_buffer[IncrementCurrentPos()];

        byte[] keyword = _keyword;

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
                    SkipDest(null, 0);
                }
                _skipDestinationIfUnknown = false;
                return RtfError.OK;
            }

            _skipDestinationIfUnknown = false;

            return DispatchKeyword(symbol, param, hasParam, null, 0);
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

            _currentPos += MinusOneIfNotSpace_8Bits(ch);
        // [FenGen:ScalarKeywordParseSection:Fast:Dest:End]

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1 && keyword[0] == (byte)'f')
            {
                _skipDestinationIfUnknown = false;
                // \f default param is 0 but param will already be 0 if we didn't parse any, so no need to set it
                fontTableKeyword = KeywordType.F;
                return RtfError.OK;
            }
            else
            {
                symbol = LookUpControlWord(keyword, keywordCount);
            }

            if (symbol == null)
            {
                if (_skipDestinationIfUnknown)
                {
                    SkipDest(null, 0);
                }
                _skipDestinationIfUnknown = false;
                return RtfError.OK;
            }

            _skipDestinationIfUnknown = false;

            fontTableKeyword = symbol.KeywordType;
            return fontTableKeyword < KeywordType.F
                ? DispatchKeyword(symbol, param, hasParam, keyword, keywordCount)
                : RtfError.OK;
        }
    }
}
