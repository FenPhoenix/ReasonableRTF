#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private RtfError ParseKeyword_FontTable_Fast_Vector128(out KeywordType fontTableKeyword, out int param)
    {
        bool hasParam = false;
        param = 0;
        Symbol? symbol;
        fontTableKeyword = default;

        int startingCurrentPos = _currentPos;

        char ch = (char)_buffer[IncrementCurrentPos()];

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
        }
        else
        {
            Vector128<byte> keyword = Vector128.Create(_buffer, _currentPos - 1);
            Vector128<byte> asciiLetters = Vector128.GreaterThan((keyword | _hex20_128) - _all_a_128, _z_minus_a_128);

            uint notEqualsElements = asciiLetters.ExtractMostSignificantBits();
            byte keywordCount = (byte)BitOperations.TrailingZeroCount(notEqualsElements);

            Vector128<byte> maskVec = Vector128.GreaterThan(Vector128.Create(keywordCount), _indexVec_128);
            keyword = Vector128.BitwiseAnd(keyword, maskVec);

            // 99.9% of keywords in the test set (849,098 out of 849,948) are less than 16 chars long, so this
            // slightly inefficient fallback path will hardly ever be hit.
            if (keywordCount >= Vector128<byte>.Count)
            {
                _currentPos = startingCurrentPos;
                return RtfError.KeywordTooLong;
            }

            _currentPos += keywordCount;
            ch = (char)_buffer[_currentPos - 1];

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
                symbol = LookUpControlWord_Vector128(keyword, keywordCount);
            }
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
            ? DispatchKeyword(symbol, param, hasParam, null, 0)
            : RtfError.OK;
    }
}
#endif
