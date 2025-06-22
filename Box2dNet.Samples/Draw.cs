using System.Numerics;
using Box2dNet.Interop;

namespace Box2dNet.Samples
{
    /// <summary>
    /// This class implements Box2D debug drawing callbacks
    /// </summary>
    internal class Draw
    {
            public Draw()
            {}

            public void Create(Camera camera)
            {}
            public void Destroy()
            {}

            public void DrawPolygon( Vector2[] vertices, int vertexCount, b2HexColor color ) 
            {}
            public void DrawSolidPolygon(b2Transform transform, Vector2[] vertices, int vertexCount, float radius, b2HexColor color )
            {}

            public void DrawCircle(Vector2 center, float radius, b2HexColor color)
            {}
            public void DrawSolidCircle(b2Transform transform, Vector2 center, float radius, b2HexColor color)
            {}

            public void DrawSolidCapsule(Vector2 p1, Vector2 p2, float radius, b2HexColor color)
            {}

            public void DrawLine(Vector2 p1, Vector2 p2, b2HexColor color)
            {}

            public void DrawTransform(b2Transform transform)
            {}

            public void DrawPoint(Vector2 p, float size, b2HexColor color)
            {}

            public void DrawString(int x, int y, params string[] s)
            {}

            public void DrawString(Vector2 p, params string[] s)
            {}

            public void DrawBounds(b2AABB aabb, b2HexColor color)
            {}

            public void Flush()
            {}
            public void DrawBackground()
            {}

            public Camera m_camera;
            public bool m_showUI;
            //public struct GLBackground* m_background;
            //public struct GLPoints* m_points;
            //public struct GLLines* m_lines;
            //public struct GLCircles* m_circles;
            //public struct GLSolidCircles* m_solidCircles;
            //public struct GLSolidCapsules* m_solidCapsules;
            //public struct GLSolidPolygons* m_solidPolygons;
            public b2DebugDraw m_debugDraw;

            //ImFont* m_regularFont;
            //ImFont* m_mediumFont;
            //ImFont* m_largeFont;
    }
}
