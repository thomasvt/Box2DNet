using System.Numerics;

namespace Box2dNet.Samples.Graphics
{
    /// <summary>
    /// The current size of the game's view (part of the screen that renders the game).
    /// </summary>
    public readonly record struct Viewport(int Width, int Height)
    {
        public float AspectRatio => Width != 0 && Height != 0 
            ? (float)Width / Height 
            : 0f;

        public Vector2 SizeF => new (Width, Height);
    }
}
