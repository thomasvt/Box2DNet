using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Box2dNet
{
    public static class IntPtrExtensions
    {
        /// <summary>
        /// Gets a readonly span of a native array of which you know the length.
        /// </summary>
        public static unsafe ReadOnlySpan<T> NativeArrayAsSpan<T>(this IntPtr intPtr, int count) where T : struct
        {
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(intPtr.ToPointer()), count);
        }

        /// <summary>
        /// Copies the native data to a strongly typed .NET array.
        /// </summary>
        public static unsafe T[] NativeArrayToArray<T>(this IntPtr intPtr, int count) where T : struct
        {
            var array = new T[count];
            MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(intPtr.ToPointer()), count).CopyTo(array);
            return array;
        }

        /// <summary>
        /// Copies the native data to your strongly typed .NET array buffer. You must ensure the array is big enough.
        /// </summary>
        public static unsafe void NativeArrayToArray<T>(this IntPtr intPtr, int count, T[] buffer) where T : struct
        {
            MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(intPtr.ToPointer()), count).CopyTo(buffer);
        }
    }
}
