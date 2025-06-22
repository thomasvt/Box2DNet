using System.Numerics;

namespace Box2dNet.Samples.Graphics
{
    public readonly struct Triangle2
    {
        public readonly Vector2 A, B, C;

        public Triangle2(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Triangle2(Span<Vector2> corners)
        {
            if (corners.Length != 3)
                throw new Exception("A triangle must have 3 corners.");

            A = corners[0];
            B = corners[1];
            C = corners[2];
        }
    }
}
