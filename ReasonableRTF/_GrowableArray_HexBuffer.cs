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
        if (_hexBuffer_Count == _hexBuffer_Capacity)
        {
            HexBuffer_Grow(_hexBuffer_Count + 1);
        }
        _hexBuffer[_hexBuffer_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HexBuffer_EnsureCapacity(int min)
    {
        if (_hexBuffer_Capacity >= min) return;
        HexBuffer_Grow(min);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void HexBuffer_Grow(int min)
    {
        int newCapacity = _hexBuffer_Capacity * 2;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < min) newCapacity = min;

        byte[] newArray = new byte[newCapacity];
        if (_hexBuffer_Count > 0) Array.Copy(_hexBuffer, 0, newArray, 0, _hexBuffer_Count);
        _hexBuffer = newArray;
        _hexBuffer_Capacity = newCapacity;
    }

    private void HexBuffer_HardReset()
    {
        if (_hexBuffer_Capacity == _internalBufferDefaultCapacity) return;
        _hexBuffer = new byte[_internalBufferDefaultCapacity];
        _hexBuffer_Capacity = _internalBufferDefaultCapacity;
    }
}
