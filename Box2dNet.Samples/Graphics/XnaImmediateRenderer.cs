using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Box2dNet.Samples.Graphics
{
    public class XnaImmediateRenderer(GraphicsDevice graphicsDevice) : IDisposable, IImmediateRenderer
    {
        private readonly BasicEffect _effect = new(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };
        private ICamera? _camera;
        private readonly List<VertexPositionColor> _xnaVerticesBuffer = new();

        /// <summary>
        /// Clears screen, depth buffer and stencil buffer.
        /// </summary>
        public void ClearScreen()
        {
            graphicsDevice.Clear(Color.Black);
        }

        public void ClearDepthBuffer()
        {
            graphicsDevice.Clear(ClearOptions.DepthBuffer, Vector4.Zero, graphicsDevice.Viewport.MaxDepth, 0);
        }

        public void Begin(ICamera camera, bool enableDepthBuffer, bool additiveBlend, bool cullCounterClockwise)
        {
            _camera = camera;

            graphicsDevice.BlendState = additiveBlend ? BlendState.Additive : BlendState.NonPremultiplied;
            graphicsDevice.DepthStencilState = enableDepthBuffer ? DepthStencilState.Default : DepthStencilState.None;
            graphicsDevice.RasterizerState = cullCounterClockwise ? RasterizerState.CullCounterClockwise : RasterizerState.CullNone;

            _effect.View = _camera.GetViewTransform().ToXna();
            var viewport = new Viewport(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            _effect.Projection = _camera.GetProjectionTransform(viewport).ToXna();
        }

        public void Draw(in Matrix4x4 worldTransform, in Mesh mesh)
        {
            if (mesh.Triangles!.Count == 0 || _camera == null) return;

            _effect.World = worldTransform.ToXna();
            _effect.Texture = null;
            _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply(); // don't use First to prevent iterator allocation

            // effect.GraphicsDevice.SetVertexBuffer(null);

            _xnaVerticesBuffer.Clear();
            foreach (var triangle in mesh.Triangles)
            {
                var color = triangle.Color.ToXna();
                _xnaVerticesBuffer.Add(new VertexPositionColor(triangle.A.ToXna(), color));
                _xnaVerticesBuffer.Add(new VertexPositionColor(triangle.B.ToXna(), color));
                _xnaVerticesBuffer.Add(new VertexPositionColor(triangle.C.ToXna(), color));
            }
            _effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _xnaVerticesBuffer.ToArray() // don't do this for a game! reuse a single buffer.
                , 0, _xnaVerticesBuffer.Count / 3);
        }


        public void End()
        {
            _camera = null;
        }

        public Viewport Viewport => new(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

        public void Dispose()
        {
            _effect.Dispose();
        }
    }
}
