/*
 * MIT License
 * 
 * Copyright (c) 2024 Brian Tobin
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using System.Runtime.CompilerServices;
using ReasonableRTF.Helper;

namespace ReasonableRTF.Models.DataTypes
{
    internal sealed class ByteArrayWithLength
    {
        internal byte[] Array;
        internal int Length;
        internal int CurrentBufferLength;
        private readonly RtfToTextConverter _conv;

        internal ByteArrayWithLength(RtfToTextConverter conv)
        {
            Array = System.Array.Empty<byte>();
            Length = 0;
            CurrentBufferLength = 0;
            _conv = conv;
        }

        internal void Set(byte[] array, int length)
        {
            Array = array;
            Length = length;
            CurrentBufferLength = length;
        }

        /// <summary>
        /// Manually bounds-checked past <see cref="T:Length"/>.
        /// Now that we have stream support, this method should always be called for array accesses to ensure the
        /// chunks are loaded when needed. Only access <see cref="T:Array"/> directly in cases where you know for
        /// sure you don't need the chunk load triggering in your particular scenario.
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
                if (index > CurrentBufferLength - 1)
                {
                    /*
                    Putting the ThrowHelper call here makes us full speed (on the byte array path). Putting this
                    here instead loses us like 6-10% again. Even though HandleOutOfBounds() has the no inlining
                    attribute! Argh!
                    But, this system does make us a little faster than before (especially on the streaming path),
                    so hey.
                    */
                    index = HandleOutOfBounds();
                }
                return Array[index];
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int HandleOutOfBounds()
        {
            if (_conv._bufferedStream != null)
            {
                _conv._currentPos--;
                int ret = _conv.IncrementCurrentPos_Stream(_conv._currentPos);
                if (_conv._currentPos > CurrentBufferLength)
                {
                    if (_conv._groupCount > 0)
                    {
                        ThrowHelper.UnmatchedBraceException();
                    }
                    else
                    {
                        ThrowHelper.IndexOutOfRange();
                    }
                    return 0;
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                if (_conv._groupCount > 0)
                {
                    ThrowHelper.UnmatchedBraceException();
                }
                else
                {
                    ThrowHelper.IndexOutOfRange();
                }
                ThrowHelper.IndexOutOfRange();
                return 0;
            }
        }
    }
}
