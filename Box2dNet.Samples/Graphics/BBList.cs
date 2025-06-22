using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Box2dNet.Samples.Graphics
{
    /// <summary>
    /// List-like struct with zero GC pressure and direct item access. Can also be used as an unordered but fast queue using Add() and Pop() for cases where the pop order is irrelevant.
    /// </summary>
    public class BBList<T> : IDisposable
    {
        private T[] _items = [];
        public int Count { get; internal set; }

        public int Add(T item)
        {
            var idx = Count;
            if (idx < _items.Length)
            {
                Count = idx + 1;
                _items[idx] = item;
                return idx;
            }
            else
            {
                return AddWithResize(item);
            }
        }

        /// <summary>
        /// Copies the items to the end of this BBList in a single copy operation.
        /// </summary>
        public void AddRange(T[] items)
        {
            AddRange(items.AsSpan());
        }

        /// <summary>
        /// Adds the items one by one to the end of this BBList.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (items is IReadOnlyCollection<T> collection)
                EnsureCapacity(Count + collection.Count);

            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Copies the items to the end of this BBList in a single copy operation.
        /// </summary>
        public void AddRange(ReadOnlySpan<T> items)
        {
            EnsureCapacity(Count + items.Length);
            items.CopyTo(_items.AsSpan()[Count..]);
            Count += items.Length;
        }

        /// <summary>
        /// Copies the items to the end of this BBList in a single copy operation.
        /// </summary>
        public void AddRange(BBList<T> items)
        {
            AddRange(items.AsReadOnlySpan());
        }

        /// <summary>
        /// Adds uninitialized items to the GrowArray and returns a Span that contains them.
        /// </summary>
        public Span<T> AddNonInitialized(int count)
        {
            var idx = Count;
            var newCount = idx + count;

            if (newCount > _items.Length)
                GrowCapacity(newCount);

            Count = newCount; // update Count AFTER Grow, because Grow may reallocate and copy all items based on Count.

            return _items.AsSpan(idx, count);
        }

        // Non-inline from List.Add to improve its code quality as uncommon path
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int AddWithResize(T item)
        {
            Debug.Assert(Count == _items.Length);
            var count = Count;
            GrowCapacity(count + 1);
            Count = count + 1; // update Count AFTER Grow, because Grow may reallocate and copy all items based on Count.
            _items[count] = item;
            return count;
        }

        /// <summary>
        /// Increase the capacity to at least the specified <paramref name="capacity"/>.
        /// </summary>
        private void GrowCapacity(int capacity)
        {
            Debug.Assert(_items.Length < capacity);

            var newCapacity = Math.Max(capacity, 2 * _items.Length);

            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

            // If the computed capacity is still less than specified, set to the original argument.
            // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
            if (newCapacity < capacity) newCapacity = capacity;

            Capacity = newCapacity;
        }

        /// <summary>
        /// Fast remove of an item by overwriting it with the last item and shrinking the array by 1.
        /// This sacrifices the order of the items in the array but is faster because no parts of the array need to be copied around to close the gap.
        /// </summary>
        public bool SwapRemove(T item)
        {
            var idx = IndexOf(item);
            if (idx <= -1) return false;

            SwapRemoveAt(idx);
            return true;
        }

        private int IndexOf(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            for (var i = 0; i < Count; i++)
            {
                if (item.Equals(_items[i])) return i;
            }

            return -1;
        }

        /// <summary>
        /// Fast remove of a range of items by copying over an equal amount of items from the back of the array (if applicable).
        /// This is faster than filling the gap by copying a larger part of the array to maintaining original item-order.
        /// </summary>
        public void SwapRemoveAt(int idx, int count = 1)
        {
            if (idx + count > Count)
                throw new ArgumentException("The range to remove doesn't entirely fall within array bounds.");

            if (idx + count < Count)
            {
                var sourceIdx = Count - count;
                sourceIdx = Math.Max(sourceIdx, idx + count); // ensure we only copy items AFTER the delete-range.
                Array.ConstrainedCopy(_items, sourceIdx, _items, idx, Count - sourceIdx);
            }

            Count -= count;
        }

        /// <summary>
        /// Removes the last item and returns it. More performant than SwapRemoveAt.
        /// </summary>
        public T Pop()
        {
            Count--;
            return _items[Count];
        }

        public void EnsureCapacity(int capacity)
        {
            if (Capacity >= capacity) return;
            GrowCapacity(capacity);
        }

        /// <summary>
        /// Clears the collection. Optionally frees the allocated capacity memory.
        /// </summary>
        /// <param name="freeCapacity">free the memory allocated for the current capacity of this GrowArray</param>
        public void Clear(bool freeCapacity = false)
        {
            if (freeCapacity)
            {
                ArrayPool<T>.Shared.Return(_items);
                _items = [];
            }

            Count = 0;
        }

        /// <summary>
        /// Returns a new array (copy) with the items in this GrowArray.
        /// </summary>
        public T[] ToArray()
        {
            return AsReadOnlySpan().ToArray();
        }

        public int Capacity
        {
            get => _items.Length;
            private set
            {
                if (value < Count)
                    throw new Exception("Cannot set capacity lower than items in this GrowArray.");

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        var newItems = ArrayPool<T>.Shared.Rent(value);
                        if (Count > 0)
                        {
                            Array.Copy(_items, newItems, Count);
                        }

                        if (_items.Length > 0)
                            ArrayPool<T>.Shared.Return(_items);
                        _items = newItems;
                    }
                    else
                    {
                        _items = [];
                    }
                }
            }
        }

        /// <summary>
        /// The underlying array. Mind that the length of this array is often bigger than Count because it's the Capacity of this GrowArray.
        /// </summary>
        public T[] InternalArray => _items;

        /// <summary>
        /// Direct access to the items as a Span.
        /// </summary>
        public Span<T> AsSpan() => _items.AsSpan(0, Count);

        /// <summary>
        /// Direct access to the items as a Span.
        /// </summary>
        public ReadOnlySpan<T> AsReadOnlySpan() => _items.AsSpan(0, Count);

        /// <summary>
        /// Direct access to the items as an <see cref="ArraySegment{T}"/>.
        /// </summary>
        public ArraySegment<T> AsArraySegment() => new(_items, 0, Count);

        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            foreach (var i in AsSpan())
            {
                if (item.Equals(i)) return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (_items.Length > 0)
            {
                ArrayPool<T>.Shared.Return(_items);
            }
        }
    }
}