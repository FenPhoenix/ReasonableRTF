#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private RtfError ParseKeyword_FontTable_Fast_Vector128(ref byte bufferRef, out KeywordType fontTableKeyword, out int param)
    {
        bool hasParam = false;
        param = 0;
        fontTableKeyword = default;

        int startingCurrentPos = _currentPos;

        char ch = (char)GetByteAtPos(ref bufferRef, startingCurrentPos);

        if (!CharExtension.IsAsciiLetter(ch))
        {
            ++_currentPos;

            return HandleControlChar(ref bufferRef, ch);
        }
        else
        {
            Symbol? symbol;
            Vector128<byte> keyword = Vector128.LoadUnsafe(ref GetRefAtPos(ref bufferRef, startingCurrentPos));
            Vector128<byte> asciiLetters = Vector128.GreaterThan((keyword | _hex20_128) - _all_a_128, _z_minus_a_128);

            uint notEqualsElements = asciiLetters.ExtractMostSignificantBits();
            byte keywordCount = (byte)BitOperations.TrailingZeroCount(notEqualsElements);

            // 99.9% of keywords in the test set (849,098 out of 849,948) are less than 16 chars long, so this
            // slightly inefficient fallback path will hardly ever be hit.
            if (keywordCount >= Vector128<byte>.Count)
            {
                return ParseKeyword_Fast(ref bufferRef);
            }

            Vector128<byte> maskVec = Vector128.GreaterThan(Vector128.Create(keywordCount), _indexVec_128);
            keyword = Vector128.BitwiseAnd(keyword, maskVec);

            int accumulatedPos = startingCurrentPos + keywordCount;

            ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos);

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
                    param = ch - '0';
                    ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + 1);

                    try
                    {
                        int paramLength;
                        for (paramLength = 1;
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
                // This negate is safe, because int max negated is -2147483647, and int min is -2147483648
                if (negateParam == 1) param = -param;
            }

            _currentPos = accumulatedPos + (ch == ' ' ? 1 : 0);

            ref byte keywordRef = ref GetRefAtPos(ref bufferRef, startingCurrentPos);

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1 && keywordRef == (byte)'f')
            {
                _skipDestinationIfUnknown = false;
                // \f default param is 0 but param will already be 0 if we didn't parse any, so no need to set it
                fontTableKeyword = KeywordType.F;
                return RtfError.OK;
            }
            else
            {
                symbol = LookUpControlWord_Vector128(keyword, ref keywordRef, keywordCount);
            }

            if (symbol == null)
            {
                if (_skipDestinationIfUnknown)
                {
                    SkipDest(ref bufferRef);
                    _skipDestinationIfUnknown = false;
                }
                return RtfError.OK;
            }

            _skipDestinationIfUnknown = false;

            fontTableKeyword = symbol.KeywordType;
            return fontTableKeyword < KeywordType.F
                ? DispatchKeyword(ref bufferRef, symbol, param, hasParam)
                : RtfError.OK;
        }
    }
}
#endif
