// ReSharper disable InconsistentNaming

using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Box2dNet.Interop
{

    /// <param name="vertices">(Original C type: const b2Vec2*)</param>
    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawPolygon(IntPtr vertices, int vertexCount, b2HexColor color, IntPtr context);

    /// <param name="vertices">(Original C type: const b2Vec2*)</param>
    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawSolidPolygon(b2Transform transform, IntPtr vertices, int vertexCount, float radius, b2HexColor color, IntPtr context);

    /// <param name="center">(Original C type: b2Vec2)</param>
    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawCircle(Vector2 center, float radius, b2HexColor color, IntPtr context);

    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawSolidCircle(b2Transform transform, float radius, b2HexColor color, IntPtr context);

    /// <param name="p1">(Original C type: b2Vec2)</param>
    /// /// <param name="p2">(Original C type: b2Vec2)</param>
    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawSolidCapsule(Vector2 p1, Vector2 p2, float radius, b2HexColor color, IntPtr context);

    /// <param name="p1">(Original C type: b2Vec2)</param>
    /// /// <param name="p2">(Original C type: b2Vec2)</param>
    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawSegment(Vector2 p1, Vector2 p2, b2HexColor color, IntPtr context);

    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawTransform(b2Transform transform, IntPtr context);

    /// <param name="p">(Original C type: b2Vec2)</param>
    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawPoint(Vector2 p, float size, b2HexColor color, IntPtr context);

    /// <param name="p">(Original C type: b2Vec2)</param>
    /// <param name="s">(Original C type: const char*)</param>
    /// <param name="context">(Original C type: void*)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DrawString(Vector2 p, IntPtr s, b2HexColor color, IntPtr context);

    /// This struct holds callbacks you can implement to draw a Box2D world.
    /// This structure should be zero initialized.
    /// @ingroup world
    public struct b2DebugDraw
    {
        /// <summary>
        /// Draw a closed polygon provided in CCW order.
        /// (Original C type: DrawPolygon*)
        /// </summary>
        public IntPtr DrawPolygon;

        /// <summary>
        /// Draw a solid closed polygon provided in CCW order.
        /// (Original C type: DrawSolidPolygon*)
        /// </summary>
        public IntPtr DrawSolidPolygon;

        /// <summary>
        /// Draw a circle.
        /// (Original C type: DrawCircle*)
        /// </summary>
        public IntPtr DrawCircle;

        /// <summary>
        /// Draw a solid circle.
        /// (Original C type: DrawSolidCircle*)
        /// </summary>
        public IntPtr DrawSolidCircle;

        /// <summary>
        /// Draw a solid capsule.
        /// (Original C type: DrawSolidCapsule*)
        /// </summary>
        public IntPtr DrawSolidCapsule;

        /// <summary>
        /// Draw a line segment.
        /// (Original C type: DrawSegment*)
        /// </summary>
        public IntPtr DrawSegment;

        /// <summary>
        /// Draw a transform. Choose your own length scale.
        /// (Original C type: DrawTransform*)
        /// </summary>
        public IntPtr DrawTransform;

        /// <summary>
        /// Draw a point.
        /// (Original C type: DrawPoint*)
        /// </summary>
        public IntPtr DrawPoint;

        /// <summary>
        /// Draw a string in world space
        /// (Original C type: DrawString*)
        /// </summary>
        public IntPtr DrawString;

        /// Bounds to use if restricting drawing to a rectangular region
        public b2AABB drawingBounds;

        /// Option to restrict drawing to a rectangular region. May suffer from unstable depth sorting.
        [MarshalAs(UnmanagedType.U1)] public bool useDrawingBounds;

        /// Option to draw shapes
        [MarshalAs(UnmanagedType.U1)] public bool drawShapes;

        /// Option to draw joints
        [MarshalAs(UnmanagedType.U1)] public bool drawJoints;

        /// Option to draw additional information for joints
        [MarshalAs(UnmanagedType.U1)] public bool drawJointExtras;

        /// Option to draw the bounding boxes for shapes
        [MarshalAs(UnmanagedType.U1)] public bool drawBounds;

        /// Option to draw the mass and center of mass of dynamic bodies
        [MarshalAs(UnmanagedType.U1)] public bool drawMass;

        /// Option to draw body names
        [MarshalAs(UnmanagedType.U1)] public bool drawBodyNames;

        /// Option to draw contact points
        [MarshalAs(UnmanagedType.U1)] public bool drawContacts;

        /// Option to visualize the graph coloring used for contacts and joints
        [MarshalAs(UnmanagedType.U1)] public bool drawGraphColors;

        /// Option to draw contact normals
        [MarshalAs(UnmanagedType.U1)] public bool drawContactNormals;

        /// Option to draw contact normal impulses
        [MarshalAs(UnmanagedType.U1)] public bool drawContactImpulses;

        /// Option to draw contact feature ids
        [MarshalAs(UnmanagedType.U1)] public bool drawContactFeatures;

        /// Option to draw contact friction impulses
        [MarshalAs(UnmanagedType.U1)] public bool drawFrictionImpulses;

        /// Option to draw islands as bounding boxes
        [MarshalAs(UnmanagedType.U1)] public bool drawIslands;

        /// User context that is passed as an argument to drawing callback functions
        public IntPtr context;
    }

    public static partial class B2Api
    {
        /// <summary>
        /// Call this to draw shapes and other debug draw data
        /// </summary>
        /// <param name="draw">(Original C type: b2DebugDraw*)</param>
        [DllImport(Box2DLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void b2World_Draw(b2WorldId worldId, ref b2DebugDraw draw);

        /// Use this to initialize your drawing interface. This allows you to implement a sub-set
        /// of the drawing functions.
        [DllImport(Box2DLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern b2DebugDraw b2DefaultDebugDraw();
    }

    public static class ColorExtensions
    {
        /// <summary>
        /// Returns the Box2D color as a .NET framework <see cref="Color"/>.
        /// </summary>
        public static Color ToDotNet(this b2HexColor color, byte alpha = 255)
        {
            return Color.FromArgb((int)(((uint)color) | (uint)(alpha << 24)));
        }
    }
}
