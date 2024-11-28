using System.Runtime.InteropServices;

namespace Box2dNet;

/// <summary>
/// Helper methods to manage handles for CLR objects so you can pass then as IntPtr to native code and back.
/// </summary>
public static class NativeHandle
{
    /// <summary>
    /// Allocates a GCHandle and returns it as an IntPtr. Don't forget to Free it!
    /// </summary>
    public static IntPtr Alloc(object @object)
    {
        return GCHandle.ToIntPtr(GCHandle.Alloc(@object));
    }

    /// <summary>
    /// Frees a previously allocated handle.
    /// </summary>
    public static void Free(IntPtr intPtr)
    {
        GCHandle.FromIntPtr(intPtr).Free();
    }

    /// <summary>
    /// Returns the managed Object associated with a handle that you created earlier.
    /// </summary>
    public static T GetObject<T>(IntPtr intPtr)
    {
        var obj = GCHandle.FromIntPtr(intPtr).Target;
#if DEBUG
        if (obj is not T t) throw new Exception($"Native pointer does not point to a '{typeof(T).Name}'.");
        return t;
#else
        return (T)obj;
#endif
    }
}