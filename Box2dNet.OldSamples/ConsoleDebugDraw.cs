using System.Numerics;
using System.Runtime.InteropServices;
using Box2dNet.Interop;

namespace Box2dNet.OldSamples
{
    internal class ConsoleDebugDraw
    {
        public static b2DebugDraw Create()
        {
            var debugDraw = B2Api.b2DefaultDebugDraw();
            debugDraw.DrawPolygon = Marshal.GetFunctionPointerForDelegate((DrawPolygon)DrawPolygon);
            debugDraw.DrawSolidPolygon = Marshal.GetFunctionPointerForDelegate((DrawSolidPolygon)DrawSolidPolygon);
            debugDraw.DrawCircle = Marshal.GetFunctionPointerForDelegate((DrawCircle)DrawCircle);
            debugDraw.DrawSolidCircle = Marshal.GetFunctionPointerForDelegate((DrawSolidCircle)DrawSolidCircle);
            debugDraw.DrawSolidCapsule = Marshal.GetFunctionPointerForDelegate((DrawSolidCapsule)DrawSolidCapsule);
            debugDraw.DrawSegment = Marshal.GetFunctionPointerForDelegate((DrawSegment)DrawSegment);
            debugDraw.DrawTransform = Marshal.GetFunctionPointerForDelegate((DrawTransform)DrawTransform);
            debugDraw.DrawPoint = Marshal.GetFunctionPointerForDelegate((DrawPoint)DrawPoint);
            debugDraw.DrawString = Marshal.GetFunctionPointerForDelegate((DrawString)DrawString);

            debugDraw.drawShapes = true;
            debugDraw.drawJoints = true;
            debugDraw.drawJointExtras = true;
            debugDraw.drawBounds = true;
            debugDraw.drawMass = true;
            debugDraw.drawBodyNames = true;
            debugDraw.drawContactPoints = true;
            debugDraw.drawGraphColors = true;
            debugDraw.drawContactNormals = true;
            debugDraw.drawContactForces = true;
            debugDraw.drawContactFeatures = true;
            debugDraw.drawFrictionForces = true;
            debugDraw.drawIslands = true;

            return debugDraw;
        }

        private static void DrawPolygon(IntPtr vertices, int vertexCount, b2HexColor color, IntPtr context)
        {
            var verticesClr = vertices.NativeArrayToArray<Vector2>(vertexCount);
            Console.WriteLine($"DrawPolygon(vertices={string.Join(',', verticesClr)}, color={color})");
        }

        private static void DrawSolidPolygon(b2Transform transform, IntPtr vertices, int vertexCount, float radius, b2HexColor color, IntPtr context)
        {
            var verticesClr = vertices.NativeArrayToArray<Vector2>(vertexCount);
            Console.WriteLine($"DrawSolidPolygon(vertices={string.Join(',', verticesClr)}, radius={radius}, transform=(p={transform.p}, q={transform.q}), color={color})");
        }

        private static void DrawCircle(Vector2 center, float radius, b2HexColor color, IntPtr context)
        {
            Console.WriteLine($"Circle(center={center}, radius={radius}, color={color})");
        }

        private static void DrawSolidCircle(b2Transform transform, float radius, b2HexColor color, IntPtr context)
        {
            Console.WriteLine($"SolidCircle(p={transform.p}, q={transform.q}, radius={radius}, color={color})");
        }

        private static void DrawSolidCapsule(Vector2 p1, Vector2 p2, float radius, b2HexColor color, IntPtr context)
        {
            Console.WriteLine($"DrawSolidCapsule(p1={p1}, p2={p2}, color={color})");
        }

        private static void DrawSegment(Vector2 p1, Vector2 p2, b2HexColor color, IntPtr context)
        {
            Console.WriteLine($"DrawSegment(p1={p1}, p2={p2}, color={color})");
        }

        private static void DrawTransform(b2Transform transform, IntPtr context)
        {
            Console.WriteLine($"DrawTransform(p={transform.p}, q={transform.q})");
        }

        private static void DrawPoint(Vector2 p, float size, b2HexColor color, IntPtr context)
        {
            Console.WriteLine($"DrawPoint(p={p}, size={size}, color={color})");
        }

        private static void DrawString(Vector2 p, IntPtr s, b2HexColor color, IntPtr context)
        {
            Console.WriteLine($"DrawString(p={p}, s='{Marshal.PtrToStringAnsi(s)}', color={color})");
        }
    }
}
