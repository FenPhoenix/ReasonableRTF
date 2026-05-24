using System.Runtime.CompilerServices;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private byte[] _hexBuffer = new byte[_internalBufferDefaultCapacity];
    private int _hexBuffer_Count;
    private int _hexBuffer_Capacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HexBuffer_Add(byte item)
    {
        HexBuffer_EnsureExtraCapacity(1);
        _hexBuffer[_hexBuffer_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HexBuffer_EnsureExtraCapacity(int minExtra)
    {
        int min = _hexBuffer_Count + minExtra;
        if (_hexBuffer_Capacity >= min) return;
        HexBuffer_Grow(min);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void HexBuffer_Grow(int min)
    {
        int newCapacity = _hexBuffer_Capacity == 0 ? 4 : _hexBuffer_Capacity * 2;
        if ((uint)newCapacity > 2146435071U) newCapacity = 2146435071;
        if (newCapacity < min) newCapacity = min;
        HexBuffer_SetCapacity(newCapacity);
    }

    private void HexBuffer_SetCapacity(int value)
    {
        if (value == _hexBuffer_Capacity) return;
        if (value > 0)
        {
            byte[] objArray = new byte[value];
            if (_hexBuffer_Count > 0) Array.Copy(_hexBuffer, 0, objArray, 0, _hexBuffer_Count);
            _hexBuffer = objArray;
            _hexBuffer_Capacity = value;
            if (_hexBuffer_Capacity < _hexBuffer_Count)
            {
                _hexBuffer_Count = _hexBuffer_Capacity;
            }
        }
        else
        {
            _hexBuffer = Array.Empty<byte>();
            _hexBuffer_Capacity = 0;
            _hexBuffer_Count = 0;
        }
    }

    private void HexBuffer_HardReset()
    {
        _hexBuffer_Count = 0;
        HexBuffer_SetCapacity(_internalBufferDefaultCapacity);
    }
}
