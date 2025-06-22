using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace Box2dNet.Samples.Graphics
{
    public interface IImmediateRenderer
    {
        void Begin(ICamera camera, bool enableDepthBuffer, bool additiveBlend, bool cullCounterClockwise);

        void Draw(in Matrix4x4 worldTransform, in Mesh mesh);

        void End();
        Viewport Viewport { get; }
        void ClearScreen();
        void ClearDepthBuffer();
    }
}
