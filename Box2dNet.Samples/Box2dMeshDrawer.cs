using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Box2dNet.Interop;
using Box2dNet.Samples.Graphics;

namespace Box2dNet.Samples
{
    /// <summary>
    /// implements Box2D debug drawing callbacks to fill a triangle <see cref="Mesh"/> that can be rendered by MonoGame (or most other rendering systems).
    /// </summary>
    internal class Box2dMeshDrawer
    {
        private const float LineHalfWidth = 0.05f;

        /// <summary>
        /// Triangle mesh to be rendered. This hooks into a copied part of my game engine to draw the Box2D outputs as triangles for easy rendering in any rendering system (eg. MonoGame).
        /// </summary>
        public readonly static Mesh Mesh = new();
        private readonly static IntPtr _meshHandle;

        static Box2dMeshDrawer()
        {
            //m_showUI = true;
            //m_debugDraw = new();

            _meshHandle = NativeHandle.Alloc(Mesh);
        }

        public static void DrawPolygon(IntPtr vertices, int vertexCount, b2HexColor color, IntPtr context)
        {
            var verticesSpan = vertices.NativeArrayAsSpan<Vector2>(vertexCount);
            // var mesh = NativeHandle.GetObject<Mesh>(context);
            Mesh.PolygonEdges(Polygon8.FromSpan(verticesSpan), LineHalfWidth, 0, Color.FromArgb((int)color));
        }

        public static void DrawSolidPolygon(b2Transform transform, IntPtr vertices, int vertexCount, float radius, b2HexColor color, IntPtr context)
        {

        }

        public static void DrawCircle(Vector2 center, float radius, b2HexColor color, IntPtr context)
        { }
        public static void DrawSolidCircle(b2Transform transform, float radius, b2HexColor color, IntPtr context)
        { }

        public static void DrawSolidCapsule(Vector2 p1, Vector2 p2, float radius, b2HexColor color, IntPtr context)
        { }

        public static void DrawSegment(Vector2 p1, Vector2 p2, b2HexColor color, IntPtr context)
        { }

        public static void DrawTransform(b2Transform transform, IntPtr context)
        { }

        public static void DrawPoint(Vector2 p, float size, b2HexColor color, IntPtr context)
        { }

        public static void DrawString(Vector2 p, IntPtr s, b2HexColor color, IntPtr context)
        { }

        public static void DrawString(Vector2 p, params string[] s)
        { }

        public static void DrawWorld(b2WorldId worldId)
        {
            b2AABB bounds = new(new(-float.MinValue, -float.MaxValue), new(float.MinValue, float.MaxValue));

            var debugDraw = new b2DebugDraw
            {
                DrawPolygon = Marshal.GetFunctionPointerForDelegate((DrawPolygon)DrawPolygon),
                DrawSolidPolygon = Marshal.GetFunctionPointerForDelegate((DrawSolidPolygon)DrawSolidPolygon),
                DrawCircle = Marshal.GetFunctionPointerForDelegate((DrawCircle)DrawCircle),
                DrawSolidCircle = Marshal.GetFunctionPointerForDelegate((DrawSolidCircle)DrawSolidCircle),
                DrawSolidCapsule = Marshal.GetFunctionPointerForDelegate((DrawSolidCapsule)DrawSolidCapsule),
                DrawSegment = Marshal.GetFunctionPointerForDelegate((DrawSegment)DrawSegment),
                DrawTransform = Marshal.GetFunctionPointerForDelegate((DrawTransform)DrawTransform),
                DrawPoint = Marshal.GetFunctionPointerForDelegate((DrawPoint)DrawPoint),
                DrawString = Marshal.GetFunctionPointerForDelegate((DrawString)DrawString),
                drawingBounds = bounds,
                useDrawingBounds = false,
                drawShapes = true,
                drawJoints = true,
                drawJointExtras = false,
                drawBounds = false,
                drawMass = false,
                drawContacts = false,
                drawGraphColors = false,
                drawContactNormals = false,
                drawContactImpulses = false,
                drawContactFeatures = false,
                drawFrictionImpulses = false,
                drawIslands = false,
                context = _meshHandle
            };

            B2Api.b2World_Draw(worldId, ref debugDraw);
        }

        public static void Clear()
        {
            Mesh.Clear();
        }

        public static void Dispose()
        {
            NativeHandle.Free(_meshHandle);
        }
    }
}
