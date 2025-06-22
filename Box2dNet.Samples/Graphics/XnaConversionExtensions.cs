using System.Drawing;
using System.Numerics;
using Xna = Microsoft.Xna.Framework;

namespace Box2dNet.Samples.Graphics
{
    public static class XnaConversionExtensions
    {
        public static Xna.Vector2 ToXna(this Vector2 v)
        {
            return new Xna.Vector2(v.X, v.Y);
        }

        public static Xna.Vector3 ToXna(this Vector3 v)
        {
            return new Xna.Vector3(v.X, v.Y, v.Z);
        }

        public static Vector2 ToNumerics(this Xna.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2 XY(this Xna.Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Xna.Matrix ToXna(this Matrix3x2 m)
        {
            return new Xna.Matrix(
                m.M11, m.M12, 0, 0, 
                m.M21, m.M22, 0, 0, 
                0, 0, 1, 0, 
                m.M31, m.M32, 0, 1);
        }

        public static Xna.Matrix ToXna(this Matrix4x4 m)
        {
            return new Xna.Matrix(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
                );
        }

        public static Xna.Color ToXna(this Color c)
        {
            return new Xna.Color(c.R, c.G, c.B, c.A);
        }
    }
}
