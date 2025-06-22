using System.Numerics;

namespace Box2dNet.Samples.Graphics
{
    public enum OriginPosition
    {
        Center,
        TopLeftCorner
    }

    public class OrthographicCamera : ICamera
    {
        private Vector3 _lookAt, _lookAtUp;

        public void LookAt(Vector3 target, Vector3 up)
        {
            _lookAt = target;
            _lookAtUp = up;
        }

        public Vector3 Position { get; set; }

        public float NearPlane { get; set; } = 0;

        public float FarPlane { get; set; } = 1;

        public OriginPosition Origin { get; set; }

        /// <summary>
        /// How many world units fit on one screen's height?
        /// </summary>
        public float ViewHeight { get; set; } = 600f;

        public Matrix4x4 GetViewTransform()
        {
            return Matrix4x4.CreateLookAt(Position, _lookAt, _lookAtUp);
        }

        public Matrix4x4 GetProjectionTransform(in Viewport viewport)
        {
            if (Origin == OriginPosition.Center)
                return Matrix4x4.CreateOrthographic(ViewHeight * viewport.AspectRatio, ViewHeight, NearPlane, FarPlane);

            var width = ViewHeight * viewport.AspectRatio;
            return Matrix4x4.CreateOrthographicOffCenter(0, width, ViewHeight, 0, NearPlane, FarPlane);
        }
    }
}
