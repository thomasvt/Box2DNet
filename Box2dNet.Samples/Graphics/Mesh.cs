using System.Drawing;
using System.Numerics;
using Box2dNet.Interop;

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


        public Mesh QuadEdges(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float halfWidth, in float z, Color color)
        {
            Line(a, b, halfWidth, z, color);
            Line(b, c, halfWidth, z, color);
            Line(c, d, halfWidth, z, color);
            Line(d, a, halfWidth, z, color);
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

        public Mesh Circle(in Vector2 center, in float radius, in int segmentCount, in float z, in Color color)
        {
            var angleStep = MathF.Tau / segmentCount;

            var p0 = center;
            for (var i = 0; i < segmentCount; i++)
            {
                var a1 = i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                var a2 = a1 + angleStep;
                var p2 = new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;

                Triangle(new Triangle2(p0, p0+p1, p0+p2), z, color);
            }
            return this;
        }

        public Mesh CirclePie(in Vector2 center, in float radius, in float offsetRadians, in float pieSizeRadians, in int segmentCount, in float z, in Color color)
        {
            var angleStep = pieSizeRadians / segmentCount;

            var p0 = center;
            for (var i = 0; i < segmentCount; i++)
            {
                var a1 = offsetRadians + i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                var a2 = a1 + angleStep;
                var p2 = new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;

                Triangle(new Triangle2(p0, p0 + p2, p0 + p1), z, color);
            }
            return this;
        }

        public Mesh CirclePieEdges(in Vector2 center, in float radius, in float offsetRadians, in float pieSizeRadians, in int segmentCount, float halfWidth, in float z, in Color color)
        {
            var angleStep = pieSizeRadians / segmentCount;

            var p0 = center;
            for (var i = 0; i < segmentCount; i++)
            {
                var a1 = offsetRadians + i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                var a2 = a1 + angleStep;
                var p2 = new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;

                Line(p0 + p1, p0 + p2, halfWidth, z, color);
            }
            return this;
        }

        public Mesh CircleEdges(in Vector2 center, in float radius, in int segmentCount, in float halfLineWidth, in float z, in Color color)
        {
            var angleStep = MathF.Tau / segmentCount;

            for (var i = 0; i < segmentCount; i++)
            {
                var a1 = i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                var a2 = a1 + angleStep;
                var p2 = new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;

                Line(center + p1, center + p2, halfLineWidth, z, color);
            }
            return this;
        }

        public Mesh Square(in Vector2 p, in float size, in float z, in Color color)
        {
            return Quad(p + new Vector2(-size, -size), p + new Vector2(-size, size), p + new Vector2(size, size), p + new Vector2(size, -size), z, color);
        }

        public Mesh Capsule(in Vector2 a, in Vector2 b, in float radius, in float z, in Color color)
        {
            var lengthNorm = (b - a).NormalizeOrZero();
            var crossNorm = lengthNorm.CrossRight();

            var cross = crossNorm * radius;

            var crossAngle = MathF.Atan2(crossNorm.Y, crossNorm.X);

            return Quad(a - cross, b - cross, b + cross, a + cross, z, color)
                .CirclePie(b, radius, crossAngle, MathF.PI, 3, z, color)
                .CirclePie(a, radius, crossAngle + MathF.PI, MathF.PI, 3, z, color);
        }

        public Mesh CapsuleEdges(in Vector2 a, in Vector2 b, in float radius, in float halfWidth, in float z, in int segmentCount, in Color color)
        {
            var lengthNorm = (b - a).NormalizeOrZero();
            var crossNorm = lengthNorm.CrossRight();

            var cross = crossNorm * radius;

            var crossAngle = MathF.Atan2(crossNorm.Y, crossNorm.X);

            return Line(a - cross, b - cross, halfWidth, z, color)
                .Line(b + cross, a + cross, halfWidth, z, color)
                .CirclePieEdges(b, radius, crossAngle, MathF.PI, segmentCount, halfWidth, z, color)
                .CirclePieEdges(a, radius, crossAngle + MathF.PI, MathF.PI, segmentCount, halfWidth, z, color);
        }
    }
}
