using System.Numerics;
using System.Runtime.InteropServices;

namespace Box2dNet.Samples.Graphics
{
    /// <summary>
    /// Polygon with up to 8 corners as a value-type (inlined in this struct, no array on the heap)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Polygon8
    {
        public int Count;

        private Vector2 P0;
        private Vector2 P1;
        private Vector2 P2;
        private Vector2 P3;
        private Vector2 P4;
        private Vector2 P5;
        private Vector2 P6;
        private Vector2 P7;

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Count = 3;
            P0 = p0; P1 = p1; P2 = p2;
            P3 = P4 = P5 = P6 = P7 = default;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Count = 4;
            P0 = p0; P1 = p1; P2 = p2; P3 = p3;
            P4 = P5 = P6 = P7 = default;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Count = 5;
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4;
            P5 = P6 = P7 = default;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5)
        {
            Count = 6;
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5;
            P6 = P7 = default;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6)
        {
            Count = 7;
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; P6 = p6;
            P7 = default;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6, Vector2 p7)
        {
            Count = 8;
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; P6 = p6; P7 = p7;
        }

        /// <summary>
        /// Access an individual corner of the polygon. For bulk access, use AsSpan() or AsReadOnlySpan().
        /// </summary>
        public Vector2 this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfRangeException();

                fixed (Vector2* basePtr = &P0)
                    return basePtr[index];
            }
            set
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfRangeException();

                fixed (Vector2* basePtr = &P0)
                    basePtr[index] = value;
            }
        }

        public readonly Span<Vector2> AsSpan()
        {
            fixed (Vector2* basePtr = &P0)
                return new Span<Vector2>(basePtr, Count);
        }

        public readonly ReadOnlySpan<Vector2> AsReadOnlySpan()
        {
            fixed (Vector2* basePtr = &P0)
                return new ReadOnlySpan<Vector2>(basePtr, Count);
        }

        /// <summary>
        /// Returns a new tranformed polygon.
        /// </summary>
        public readonly Polygon8 Transform(in Matrix3x2 transform)
        {
            // this assumes polygons are always at least 3 corners, but it doesn't break when it's less.
            var result = new Polygon8
            {
                Count = Count,
                P0 = Vector2.Transform(P0, transform),
                P1 = Vector2.Transform(P1, transform),
                P2 = Vector2.Transform(P2, transform)
            };

            if (Count <= 3) return result; result.P3 = Vector2.Transform(P3, transform);
            if (Count == 4) return result; result.P4 = Vector2.Transform(P4, transform);
            if (Count == 5) return result; result.P5 = Vector2.Transform(P5, transform);
            if (Count == 6) return result; result.P6 = Vector2.Transform(P6, transform);
            if (Count == 7) return result; result.P7 = Vector2.Transform(P7, transform);
            return result;
        }

        /// <summary>
        /// Returns a new translated polygon.
        /// </summary>
        public Polygon8 Tranlate(in Vector2 translation)
        {
            // this assumes polygons are always at least 3 corners, but it doesn't break when it's less.
            var result = new Polygon8
            {
                Count = Count,
                P0 = P0 + translation,
                P1 = P1 + translation,
                P2 = P2 + translation
            };

            if (Count <= 3) return result; result.P3 = P3 + translation;
            if (Count == 4) return result; result.P4 = P4 + translation;
            if (Count == 5) return result; result.P5 = P5 + translation;
            if (Count == 6) return result; result.P6 = P6 + translation;
            if (Count == 7) return result; result.P7 = P7 + translation;
            return result;
        }

        public readonly void FillArray(in Vector2[] array)
        {
            if (array.Length < Count) throw new ArgumentException($"Array is too small to contain all {Count} corners of this polygon.");
            array[0] = P0; 
            array[1] = P1; 
            array[2] = P2;
            if (Count <= 3) return;
            array[3] = P3;
            if (Count == 4) return;
            array[4] = P4;
            if (Count == 5) return;
            array[5] = P5;
            if (Count == 6) return;
            array[6] = P6;
            if (Count == 7) return;
            array[7] = P7;
        }

        public override string ToString()
        {
            var a = this;
            return string.Join(" ", Enumerable.Range(0, Count).Select(i => a[i]));
        }

        public readonly Polygon8 Rotate(float radians)
        {
            return Transform(Matrix3x2.CreateRotation(radians));
        }

        public readonly Polygon8 Scale(in float factor)
        {
            // this assumes polygons are always at least 3 corners, but it doesn't break when it's less.
            var result = new Polygon8
            {
                Count = Count,
                P0 = P0 * factor,
                P1 = P1 * factor,
                P2 = P2 * factor
            };

            if (Count <= 3) return result; result.P3 = P3 * factor;
            if (Count == 4) return result; result.P4 = P4 * factor;
            if (Count == 5) return result; result.P5 = P5 * factor;
            if (Count == 6) return result; result.P6 = P6 * factor;
            if (Count == 7) return result; result.P7 = P7 * factor;
            return result;
        }

        public static Polygon8 Square(float size)
        {
            var hs = size * 0.5f;
            return new Polygon8(new(-hs, -hs), new(-hs, hs), new(hs, hs), new(hs, -hs));
        }

        public static Polygon8 Quad(in Vector2 minCorner, in Vector2 maxCorner)
        {
            var minX = minCorner.X;
            var minY = minCorner.Y;
            var maxX = maxCorner.X;
            var maxY = maxCorner.Y;
            return new Polygon8(new(minX, minY), new(minX, maxY), new(maxX, maxY), new(maxX, minY));
        }

        public static Polygon8 FromSpan(ReadOnlySpan<Vector2> corners)
        {
            if (corners.Length < 3 || corners.Length > 8)
                throw new Exception("Polygon8 doesn't support polygons with < 3 or > 8 corners.");
            var p = new Polygon8
            {
                Count = corners.Length,
            };
            for (var i = 0; i < corners.Length; i++)
            {
                p[i] = corners[i];
            }
            return p;
        }
    }
}
