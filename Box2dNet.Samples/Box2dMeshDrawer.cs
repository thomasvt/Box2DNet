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
        private const float PxWorldSize = 0.05f;

        /// <summary>
        /// Triangle mesh to be rendered. This hooks into a copied part of my game engine to draw the Box2D outputs as triangles for easy rendering in any rendering system (eg. MonoGame).
        /// </summary>
        public readonly static Mesh Mesh = new();

        static Box2dMeshDrawer()
        {
            //m_showUI = true;
            //m_debugDraw = new();
        }

        public static void DrawPolygon(IntPtr vertices, int vertexCount, b2HexColor color, IntPtr context)
        {
            var verticesSpan = vertices.NativeArrayAsSpan<Vector2>(vertexCount);
            var p = Polygon8.FromSpan(verticesSpan);
            Mesh.PolygonEdges(p, PxWorldSize, 0, color.ToDotNet());

        }

        public static void DrawSolidPolygon(b2Transform transform, IntPtr vertices, int vertexCount, float radius, b2HexColor color, IntPtr context)
        {
            // todo support radius visualization.

            var verticesSpan = vertices.NativeArrayAsSpan<Vector2>(vertexCount);
            var p = Polygon8.FromSpan(verticesSpan);
            p = p.Transform(transform.ToMatrix3x2());
            Mesh.Polygon(p, 0, color.ToDotNet(128));
            Mesh.PolygonEdges(p, PxWorldSize, 0, color.ToDotNet());
        }

        public static void DrawCircle(Vector2 center, float radius, b2HexColor color, IntPtr context)
        {
            Mesh.CircleEdges(center, radius, 8, PxWorldSize, 0, color.ToDotNet());
        }

        public static void DrawSolidCircle(b2Transform transform, float radius, b2HexColor color, IntPtr context)
        {
            Mesh.Circle(transform.p, radius, 8, 0, color.ToDotNet(128));
            Mesh.CircleEdges(transform.p, radius, 8, PxWorldSize, 0, color.ToDotNet());
            Mesh.Line(transform.p, transform.p + new Vector2(transform.q.c, transform.q.s) * radius, PxWorldSize, 0, color.ToDotNet());
        }

        public static void DrawSolidCapsule(Vector2 p1, Vector2 p2, float radius, b2HexColor color, IntPtr context)
        {
            Mesh.Capsule(p1, p2, radius, 0, color.ToDotNet(128));
            Mesh.CapsuleEdges(p1, p2, radius, PxWorldSize, 0, color.ToDotNet());
        }

        public static void DrawSegment(Vector2 p1, Vector2 p2, b2HexColor color, IntPtr context)
        {
            Mesh.Line(p1, p2, PxWorldSize, 0, color.ToDotNet());
        }

        public static void DrawTransform(b2Transform transform, IntPtr context)
        { }

        public static void DrawPoint(Vector2 p, float size, b2HexColor color, IntPtr context)
        {
            Mesh.Square(p, size * PxWorldSize, 0, color.ToDotNet());
        }

        public static void DrawString(Vector2 p, IntPtr s, b2HexColor color, IntPtr context)
        { }

        public static void DrawWorld(b2WorldId worldId)
        {
            b2AABB bounds = new(new(-float.MinValue, -float.MaxValue), new(float.MinValue, float.MaxValue));

            var meshHandle = NativeHandle.Alloc(Mesh);

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
                context = meshHandle
            };

            B2Api.b2World_Draw(worldId, ref debugDraw);

            NativeHandle.Free(meshHandle);
        }

        public static void Clear()
        {
            Mesh.Clear();
        }
    }
}
