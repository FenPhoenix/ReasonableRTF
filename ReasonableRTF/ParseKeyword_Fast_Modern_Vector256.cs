#if NET8_0_OR_GREATER

using System.Numerics;
using System.Runtime.Intrinsics;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private static readonly Vector256<byte> _hex20_256 = Vector256.Create((byte)0x20);
    private static readonly Vector256<byte> _all_a_256 = Vector256.Create((byte)'a');
    private static readonly Vector256<byte> _z_minus_a_256 = Vector256.Create((byte)('z' - 'a'));

    private static readonly Vector256<byte> _indexVec_256 = Vector256.Create(
        (byte)
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
        17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
    );

    private RtfError ParseKeyword_Fast_Vector256()
    {
        bool hasParam = false;
        int param = 0;
        Symbol? symbol;

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
            Vector256<byte> keyword = Vector256.Create(_buffer, _currentPos - 1);
            Vector256<byte> asciiLetters = Vector256.GreaterThan((keyword | _hex20_256) - _all_a_256, _z_minus_a_256);

            uint notEqualsElements = asciiLetters.ExtractMostSignificantBits();
            byte keywordCount = (byte)BitOperations.TrailingZeroCount(notEqualsElements);

            Vector256<byte> maskVec = Vector256.GreaterThan(Vector256.Create(keywordCount), _indexVec_256);
            keyword = Vector256.BitwiseAnd(keyword, maskVec);

            _currentPos += keywordCount;
            ch = (char)_buffer[_currentPos - 1];

            if (keywordCount == _keywordMaxLen && CharExtension.IsAsciiLetter(ch))
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

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1 && keyword[0] == (byte)'f')
            {
                symbol = _fontSymbol;
                _skipDestinationIfUnknown = false;
                return DispatchKeyword(symbol, param, hasParam, null, 0);
            }
            else
            {
                symbol = LookUpControlWord_Vector256(keyword, keywordCount);
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

        return DispatchKeyword(symbol, param, hasParam, null, 0);
    }
}
#endif
