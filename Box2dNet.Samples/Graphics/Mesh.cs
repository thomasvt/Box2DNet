using System.Drawing;
using System.Numerics;

namespace Box2dNet.Samples.Graphics
{
    public record struct Triangle(Vector3 A, Vector3 B, Vector3 C, Color Color)
    {
        public static Triangle operator *(Triangle t, float weight)
        {
            return new Triangle(t.A * weight, t.B * weight, t.C * weight, t.Color);
        }

        public static Triangle operator +(Triangle t, Vector3 b)
        {
            return new Triangle(t.A + b, t.B + b, t.C + b, t.Color);
        }
    };

    /// <summary>
    /// Renderable triangle-based Mesh, with procedural drawing methods. Not threadsafe!
    /// </summary>
    public class Mesh
    {
        public readonly List<Triangle> Triangles = new();
        private readonly List<Triangle2> _triangleBuffer = new();

        public void Clear()
        {
            Triangles.Clear();
        }

        public Mesh Triangle(in Triangle2 t, in float z, in Color color)
        {
            Triangles.Add(new Triangle(t.A.ToVector3(z), t.B.ToVector3(z), t.C.ToVector3(z), color));
            return this;
        }

        public Mesh Quad(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float z, Color color)
        {
            Triangles.Add(new Triangle(a.ToVector3(z), b.ToVector3(z), c.ToVector3(z), color));
            Triangles.Add(new Triangle(a.ToVector3(z), c.ToVector3(z), d.ToVector3(z), color));
            return this;
        }

        public Mesh Polygon(in Polygon8 polygon, in float z, in Color color)
        {
            if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

            TriangulatorConvex.Shared.Triangulate(polygon.AsReadOnlySpan(), _triangleBuffer);
            foreach (var triangle in _triangleBuffer)
            {
                Triangle(triangle, z, color);
            }
            return this;
        }

        public Mesh PolygonEdges(in Polygon8 polygon, in float halfWidth, in float z, in Color color)
        {
            if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

            var points = polygon.AsSpan();
            for (var i = 0; i < points.Length; i++)
            {
                var p0 = points[i];
                var p1 = points[(i + 1) % points.Length];

                Line(p0, p1, halfWidth, z, color);
            }
            return this;
        }

        public Mesh Line(in Vector2 a, in Vector2 b, in float halfWidth, in float z, in Color color)
        {
            var abNorm = (b - a).NormalizeOrZero();
            var longWidth = abNorm * halfWidth;
            var latWidth = longWidth.CrossLeft();

            return Quad(a - longWidth + latWidth, b + longWidth + latWidth, b + longWidth - latWidth, a - longWidth - latWidth, z, color);
        }
    }
}
