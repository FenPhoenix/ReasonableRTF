using System.Runtime.CompilerServices;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private const int _plainTextDefaultCapacity = 4096;

    private char[] _plainText = new char[_plainTextDefaultCapacity];
    private int _plainText_Capacity;
    private int _plainText_Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PlainText_Add(char item)
    {
        PlainText_EnsureExtraCapacity(1);
        _plainText[_plainText_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PlainText_EnsureExtraCapacity(int minExtra)
    {
        int min = _plainText_Count + minExtra;
        if (_plainText_Capacity >= min) return;
        PlainText_Grow(min);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PlainText_Grow(int min)
    {
        int newCapacity = _plainText_Capacity == 0 ? 4 : _plainText_Capacity * 2;
        if ((uint)newCapacity > 2146435071U) newCapacity = 2146435071;
        if (newCapacity < min) newCapacity = min;
        PlainText_SetCapacity(newCapacity);
    }

    private void PlainText_SetCapacity(int value)
    {
        if (value == _plainText_Capacity) return;
        if (value > 0)
        {
            char[] objArray = new char[value];
            if (_plainText_Count > 0) Array.Copy(_plainText, 0, objArray, 0, _plainText_Count);
            _plainText = objArray;
            _plainText_Capacity = value;
            if (_plainText_Capacity < _plainText_Count)
            {
                _plainText_Count = _plainText_Capacity;
            }
        }
        else
        {
            _plainText = Array.Empty<char>();
            _plainText_Capacity = 0;
            _plainText_Count = 0;
        }
    }

    private void PlainText_HardReset()
    {
        _plainText_Count = 0;
        PlainText_SetCapacity(_plainTextDefaultCapacity);
    }
}
