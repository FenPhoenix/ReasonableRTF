using System.Runtime.CompilerServices;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private char[] _unicodeBuffer = new char[_internalBufferDefaultCapacity];
    private int _unicodeBuffer_Count;
    private int _unicodeBuffer_Capacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnicodeBuffer_Add(char item)
    {
        if (_unicodeBuffer_Count == _unicodeBuffer_Capacity)
        {
            UnicodeBuffer_EnsureCapacity(_unicodeBuffer_Count + 1);
        }
        _unicodeBuffer[_unicodeBuffer_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnicodeBuffer_EnsureCapacity(int min)
    {
        if (_unicodeBuffer_Capacity >= min) return;
        UnicodeBuffer_Grow(min);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void UnicodeBuffer_Grow(int min)
    {
        int newCapacity = _unicodeBuffer_Capacity == 0 ? 4 : _unicodeBuffer_Capacity * 2;
        if ((uint)newCapacity > 2146435071U) newCapacity = 2146435071;
        if (newCapacity < min) newCapacity = min;
        UnicodeBuffer_SetCapacity(newCapacity);
    }

    private void UnicodeBuffer_SetCapacity(int value)
    {
        if (value == _unicodeBuffer_Capacity) return;
        if (value > 0)
        {
            char[] objArray = new char[value];
            if (_unicodeBuffer_Count > 0) Array.Copy(_unicodeBuffer, 0, objArray, 0, _unicodeBuffer_Count);
            _unicodeBuffer = objArray;
            _unicodeBuffer_Capacity = value;
            if (_unicodeBuffer_Capacity < _unicodeBuffer_Count)
            {
                _unicodeBuffer_Count = _unicodeBuffer_Capacity;
            }
        }
        else
        {
            _unicodeBuffer = Array.Empty<char>();
            _unicodeBuffer_Capacity = 0;
            _unicodeBuffer_Count = 0;
        }
    }

    private void UnicodeBuffer_HardReset()
    {
        _unicodeBuffer_Count = 0;
        UnicodeBuffer_SetCapacity(_internalBufferDefaultCapacity);
    }
}
