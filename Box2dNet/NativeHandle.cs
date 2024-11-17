using System.Runtime.InteropServices;

namespace Box2dNet;

/// <summary>
/// Wraps a managed object into a GCHandle so you can pass it as an IntPtr to native code and back. Dispose it when the roundtripping for the object is no longer needed.
/// </summary>
public class NativeHandle<T> : IDisposable
{
    private bool _isDisposed;

    /// <summary>
    /// The managed object.
    /// </summary>
    public T Object { get; }

    /// <summary>
    /// Pass this to the native side as a handle to your managed object.
    /// </summary>
    public IntPtr IntPtr { get; }

    public NativeHandle(T @object)
    {
        Object = @object;
        IntPtr = GCHandle.ToIntPtr(GCHandle.Alloc(@object));
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        GCHandle.FromIntPtr(IntPtr).Free();
        _isDisposed = true;
    }

    /// <summary>
    /// Returns the managed Object from an intptr to a NativeHandle that you created earlier/elsewhere.
    /// </summary>
    public static T GetObjectFromIntPtr(IntPtr intPtr)
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