using System.Runtime.CompilerServices;
using ReasonableRTF.Models.DataTypes;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    // 20 bytes * 4 for up to 4 bytes per char. Chars are 2 bytes but like whatever, why do math when you can
    // over-provision.
    private const int _charGeneralBufferDefaultCapacity = 20 * 4;

    private char[] _charGeneralBuffer = new char[_charGeneralBufferDefaultCapacity];
    private int _charGeneralBuffer_Capacity;
    private int _charGeneralBuffer_Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CharGeneralBuffer_Add(char item)
    {
        if (_charGeneralBuffer_Count == _charGeneralBuffer_Capacity)
        {
            CharGeneralBuffer_EnsureCapacity(_charGeneralBuffer_Count + 1);
        }
        _charGeneralBuffer[_charGeneralBuffer_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CharGeneralBuffer_AddRange(ListFast<char> items, int count)
    {
        CharGeneralBuffer_EnsureCapacity(_charGeneralBuffer_Count + count);
        // We usually add small enough arrays that a loop is faster
        for (int i = 0; i < count; i++)
        {
            _charGeneralBuffer[_charGeneralBuffer_Count + i] = items[i];
        }
        _charGeneralBuffer_Count += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CharGeneralBuffer_EnsureCapacity(int min)
    {
        if (_charGeneralBuffer_Capacity >= min) return;
        CharGeneralBuffer_Grow(min);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void CharGeneralBuffer_Grow(int min)
    {
        int newCapacity = _charGeneralBuffer_Capacity == 0 ? 4 : _charGeneralBuffer_Capacity * 2;
        if ((uint)newCapacity > 2146435071U) newCapacity = 2146435071;
        if (newCapacity < min) newCapacity = min;
        CharGeneralBuffer_SetCapacity(newCapacity);
    }

    private void CharGeneralBuffer_SetCapacity(int value)
    {
        if (value == _charGeneralBuffer_Capacity) return;
        if (value > 0)
        {
            char[] objArray = new char[value];
            if (_charGeneralBuffer_Count > 0) Array.Copy(_charGeneralBuffer, 0, objArray, 0, _charGeneralBuffer_Count);
            _charGeneralBuffer = objArray;
            _charGeneralBuffer_Capacity = value;
            if (_charGeneralBuffer_Capacity < _charGeneralBuffer_Count)
            {
                _charGeneralBuffer_Count = _charGeneralBuffer_Capacity;
            }
        }
        else
        {
            _charGeneralBuffer = Array.Empty<char>();
            _charGeneralBuffer_Capacity = 0;
            _charGeneralBuffer_Count = 0;
        }
    }

    private void CharGeneralBuffer_HardReset(int capacity)
    {
        _charGeneralBuffer_Count = 0;
        CharGeneralBuffer_SetCapacity(capacity);
    }
}
