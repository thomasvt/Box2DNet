using System.Numerics;

namespace Box2dNet.Samples.Graphics
{
    public readonly struct Triangle3
    {
        public readonly Vector3 A, B, C;

        public Triangle3(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Triangle3(Span<Vector3> corners)
        {
            if (corners.Length != 3)
                throw new Exception("A triangle must have 3 corners.");

            A = corners[0];
            B = corners[1];
            C = corners[2];
        }
    }
}
