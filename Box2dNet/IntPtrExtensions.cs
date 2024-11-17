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
    }
}
