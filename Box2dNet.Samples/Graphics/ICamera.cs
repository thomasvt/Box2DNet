using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace Box2dNet.Samples.Graphics;

public interface ICamera
{
    Matrix4x4 GetViewTransform();
    Matrix4x4 GetProjectionTransform(in Viewport viewport);
}