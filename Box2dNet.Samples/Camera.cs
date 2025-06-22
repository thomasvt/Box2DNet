using System.Numerics;
using Box2dNet.Interop;

namespace Box2dNet.Samples
{
    internal class Camera
    {
        public Camera()
        {
            m_width = 1920;
            m_height = 1080;
            ResetView();
        }

        public void ResetView()
        {
            m_center = new(0.0f, 20.0f);
            m_zoom = 1.0f;
        }

        public Vector2 ConvertScreenToWorld(Vector2 ps)
        {
            float w = m_width;
            float h = m_height;
            float u = ps.X / w;
            float v = (h - ps.Y) / h;

            float ratio = w / h;
            Vector2 extents = new(m_zoom * ratio, m_zoom);

            Vector2 lower = m_center - extents;
            Vector2 upper = m_center + extents;

            Vector2 pw = new ((1.0f - u) * lower.X + u * upper.X, (1.0f - v) * lower.Y + v * upper.Y);
            return pw;
        }

        public Vector2 ConvertWorldToScreen(Vector2 pw)
        {
            float w = m_width;
            float h = m_height;
            float ratio = w / h;

            Vector2 extents = new (m_zoom * ratio, m_zoom);

            Vector2 lower = m_center - extents;
            Vector2 upper = m_center + extents;

            float u = (pw.X - lower.X) / (upper.X - lower.X);
            float v = (pw.Y - lower.Y) / (upper.Y - lower.Y);

            Vector2 ps = new(u * w, (1.0f - v) * h);
            return ps;
        }

        public void BuildProjectionMatrix(float[] m, float zBias)
        {
            float ratio = (m_width) / (m_height);
            Vector2 extents = new(m_zoom * ratio, m_zoom);

            Vector2 lower = m_center - extents;
            Vector2 upper = m_center + extents;
            float w = upper.X - lower.X;
            float h = upper.Y - lower.Y;

            m[0] = 2.0f / w;
            m[1] = 0.0f;
            m[2] = 0.0f;
            m[3] = 0.0f;

            m[4] = 0.0f;
            m[5] = 2.0f / h;
            m[6] = 0.0f;
            m[7] = 0.0f;

            m[8] = 0.0f;
            m[9] = 0.0f;
            m[10] = -1.0f;
            m[11] = 0.0f;

            m[12] = -2.0f * m_center.X / w;
            m[13] = -2.0f * m_center.Y / h;
            m[14] = zBias;
            m[15] = 1.0f;
        }

        public b2AABB GetViewBounds()
        {
            b2AABB bounds;
            bounds.lowerBound = ConvertScreenToWorld( new( 0.0f, (float)m_height) );
            bounds.upperBound = ConvertScreenToWorld( new( (float)m_width, 0.0f) );
            return bounds;
        }

        public Vector2 m_center;
        public float m_zoom;
        public float m_width;
        public float m_height;
    }
}
