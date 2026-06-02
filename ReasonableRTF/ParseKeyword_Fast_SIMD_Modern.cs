#if NET8_0_OR_GREATER

using System.Numerics;
using System.Runtime.Intrinsics;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private static readonly Vector128<byte> _hex20_128 = Vector128.Create((byte)0x20);
    private static readonly Vector128<byte> _all_a_128 = Vector128.Create((byte)'a');
    private static readonly Vector128<byte> _z_minus_a_128 = Vector128.Create((byte)('z' - 'a'));

    /*
    TODO: Interestingly, Vector128 is faster (on my Ryzen 5600) than Vector256 here.
    Is it due to less overlap in loading each keyword and so less duplicate data loaded / less waste?
    If we were smarter about it and parsed all found complete keywords in each vector, would Vector256 be faster
    again?
    */
    private RtfError ParseKeyword_Fast_Vector128(ref byte bufferRef)
    {
        bool hasParam = false;
        int param = 0;

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
                long longParam = ch - '0';
                ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + 1);

                int paramLength;
                for (paramLength = 1;
                     paramLength < _paramMaxLen + 1 && CharExtension.IsAsciiDigit(ch);
                     paramLength++,
                     ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + paramLength))
                {
                    longParam = (longParam * 10) + (ch - '0');
                }
                if (paramLength > _paramMaxLen || longParam > int.MaxValue)
                {
                    return RtfError.ParameterOutOfRange;
                }

                param = (int)longParam;

                accumulatedPos += paramLength;
                // This negate is safe, because int max negated is -2147483647, and int min is -2147483648
                if (negateParam == 1) param = -param;
            }

            _currentPos = accumulatedPos + (ch == ' ' ? 1 : 0);

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
                symbol = LookUpControlWord_Vector128(keyword, ref keywordRef, keywordCount);
            }

            if (symbol == null)
            {
                if (_skipDestinationIfUnknown)
                {
                    _skipDestinationIfUnknown = false;
                    SkipDest(ref bufferRef);
                }
                return RtfError.OK;
            }

            _skipDestinationIfUnknown = false;

            return DispatchKeyword(ref bufferRef, symbol, param, hasParam);
        }
    }
}
#endif
