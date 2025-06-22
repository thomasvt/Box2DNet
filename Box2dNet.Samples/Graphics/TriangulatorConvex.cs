using System.Numerics;

namespace Box2dNet.Samples.Graphics
{
    public class TriangulatorConvex
    {
        /// <summary>
        /// Single instance for reuse.
        /// </summary>
        public readonly static TriangulatorConvex Shared = new();
        
        public void Triangulate(ReadOnlySpan<Vector2> corners, List<Triangle2> triangleBuffer)
        {
            triangleBuffer.Clear();
            
            var a = corners[0];
            var b = corners[1];
            for (var i = 0; i < corners.Length - 2; i++)
            {
                var c = corners[i + 2];
                triangleBuffer.Add(new Triangle2(a, b, c));
                b = c;
            }
        }
    }
}
