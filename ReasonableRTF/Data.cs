using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ReasonableRTF;


// Class instead of enum so we don't have to keep casting its fields
internal static class ByteSize
{
    internal const int KB = 1024;
    internal const int MB = KB * 1024;
    internal const int GB = MB * 1024;
}

// TODO: Get rid of this and just take an array + length in the public Convert() method
[StructLayout(LayoutKind.Auto)]
internal readonly struct ByteArrayWithLength
{
    internal readonly byte[] Array;
    internal readonly int Length;

    public ByteArrayWithLength()
    {
        Array = System.Array.Empty<byte>();
        Length = 0;
    }

    internal ByteArrayWithLength(byte[] array)
    {
        Array = array;
        Length = array.Length;
    }

    internal ByteArrayWithLength(byte[] array, int length)
    {
        Array = array;
        Length = length;
    }

    // This MUST be a method (not a static field) to maintain performance!
    internal static ByteArrayWithLength Empty() => new();

    /// <summary>
    /// Manually bounds-checked past <see cref="T:Length"/>.
    /// If you don't need bounds-checking past <see cref="T:Length"/>, access <see cref="T:Array"/> directly.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal byte this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // Very unfortunately, we have to manually bounds-check here, because our array could be longer
            // than Length (such as when it comes from a pool).
            if (index > Length - 1) ThrowHelper.IndexOutOfRange();
            return Array[index];
        }
    }
}

// How many times have you thought, "Gee, I wish I could just reach in and grab that backing array from
// that List, instead of taking the senseless performance hit of having it copied to a newly allocated
// array all the time in a ToArray() call"? Hooray!
/// <summary>
/// Because this list exposes its internal array and also doesn't clear said array on <see cref="ClearFast"/>,
/// it must be used with care.
/// <para>
/// -Only use this with value types. Reference types will be left hanging around in the array.
/// </para>
/// <para>
/// -The internal array is there so you can get at it without incurring an allocation+copy.
///  It can very easily become desynced with the <see cref="ListFast{T}"/> if you modify it.
/// </para>
/// <para>
/// -Only use the internal array in conjunction with the <see cref="Count"/> property.
///  Using the <see cref="ItemsArray"/>.Length value will get the array's actual length, when what you
///  wanted was the list's "virtual" length. This is the same as a normal List except with a normal List
///  the array is private so you can't have that problem.
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class ListFast<T>
{
    internal T[] ItemsArray;
    private int _itemsArrayLength;

    /// <summary>
    /// Properties are slow. You can set this from outside if you know what you're doing.
    /// </summary>
    internal int Count;

    /// <summary>
    /// No bounds checking, so use caution!
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ItemsArray[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => ItemsArray[index] = value;
    }

    internal ListFast(int capacity)
    {
        ItemsArray = new T[capacity];
        _itemsArrayLength = capacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(T item)
    {
        if (Count == _itemsArrayLength) EnsureCapacity(Count + 1);
        ItemsArray[Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddRange(ListFast<T> items, int count)
    {
        EnsureCapacity(Count + count);
        // We usually add small enough arrays that a loop is faster
        for (int i = 0; i < count; i++)
        {
            ItemsArray[Count + i] = items[i];
        }
        Count += count;
    }

    /*
    Honestly, for fixed-size value types, doing an Array.Clear() is completely unnecessary. For reference
    types, you definitely want to clear it to get rid of all the references, but for ints or chars etc.,
    all a clear does is set a bunch of fixed-width values to other fixed-width values. You don't save
    space and you don't get rid of loose references, all you do is waste an alarming amount of time. We
    drop fully 200ms from the Unicode parser just by using the fast clear!
    */
    /// <summary>
    /// Just sets <see cref="Count"/> to 0. Doesn't zero out the array or do anything else whatsoever.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ClearFast() => Count = 0;

    internal int Capacity
    {
        get => _itemsArrayLength;
        set
        {
            if (value == _itemsArrayLength) return;
            if (value > 0)
            {
                T[] objArray = new T[value];
                if (Count > 0) Array.Copy(ItemsArray, 0, objArray, 0, Count);
                ItemsArray = objArray;
                _itemsArrayLength = value;
                if (_itemsArrayLength < Count) Count = _itemsArrayLength;
            }
            else
            {
                ItemsArray = Array.Empty<T>();
                _itemsArrayLength = 0;
                Count = 0;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnsureCapacity(int min)
    {
        if (_itemsArrayLength >= min) return;
        int newCapacity = _itemsArrayLength == 0 ? 4 : _itemsArrayLength * 2;
        if ((uint)newCapacity > 2146435071U) newCapacity = 2146435071;
        if (newCapacity < min) newCapacity = min;
        Capacity = newCapacity;
    }
}
