﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.IO.Pipelines
{
    /// <summary>
    /// An enumerator over the <see cref="ReadableBuffer"/>
    /// </summary>
    public struct BufferEnumerator
    {
        private object _segment;
        private int _startIndex;
        private readonly int _endIndex;
        private readonly object _endSegment;

        /// <summary>
        ///
        /// </summary>
        public BufferEnumerator(ReadCursor start, ReadCursor end)
        {
            _startIndex = start.Index;
            _segment = start.Segment;
            _endSegment = end.Segment;
            _endIndex = end.Index;
            Current = default;
        }

        /// <summary>
        /// The current <see cref="Buffer{Byte}"/>
        /// </summary>
        public Memory<byte> Current { get; set; }

        /// <summary>
        /// Moves to the next <see cref="Buffer{Byte}"/> in the <see cref="ReadableBuffer"/>
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            var segment = _segment;

            if (segment == null)
            {
                return false;
            }

            if (segment is BufferSegment bufferSegment)
            {
                var start = _startIndex;
                var end = bufferSegment.End;

                if (segment == _endSegment)
                {
                    end = _endIndex;
                    _segment = null;
                }
                else
                {
                    _segment = bufferSegment.Next;
                    if (_segment == null)
                    {
                        if (_endSegment != null)
                        {
                            ThrowEndNotSeen();
                        }
                    }
                    else
                    {
                        _startIndex = bufferSegment.Next.Start;
                    }
                }

                Current = bufferSegment.Memory.Slice(start, end - start);
            }

            if (segment is byte[] array)
            {
                Current = ((Memory<byte>)array).Slice(_startIndex, _endIndex - _startIndex);
            }

            return true;
        }

        private void ThrowEndNotSeen()
        {
            throw new InvalidOperationException("Segments ended by end was never seen");
        }

        public BufferEnumerator GetEnumerator()
        {
            return this;
        }

        public void Reset()
        {
            PipelinesThrowHelper.ThrowNotSupportedException();
        }

        public ReadCursor CreateCursor(int offset)
        {
            return new ReadCursor(_segment, _startIndex + offset);
        }
    }
}
