using System.Numerics;
using Box2dNet.Samples.Graphics;
using Box2dNet.Samples.SampleBenchmark;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

namespace Box2dNet.Samples
{
    internal class SamplesGame : Game
    {
        // --- BEGIN MonoGame bridging services:
        private readonly GraphicsDeviceManager _gdm;
        private readonly OrthographicCamera _camera = new();
        private IImmediateRenderer? _renderer;
        // --- END MonoGame bridging services
        
        private SampleContext s_context = new();
        private Sample? s_sample;

        public SamplesGame()
        {
            _gdm = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080
            };

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1 / s_context.hertz);
        }

        private static SampleEntry[] g_sampleEntries = [
            new("Benchmark", "Spinner", BenchmarkSpinner.Create)
        ];

        protected override void LoadContent()
        {
            base.LoadContent();

            _renderer = new XnaImmediateRenderer(GraphicsDevice);

            _camera.FarPlane = 2;
            _camera.Position = new Vector3(0, 0, 1);
            _camera.LookAt(new Vector3(0,0,0), Vector3.UnitY);
            _camera.ViewHeight = GraphicsDevice.Viewport.Height;

            SwitchSample(0);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Box2dMeshDrawer.Clear();
            s_sample?.Step(); // steps and draws to the triangle Mesh.

            TargetElapsedTime = TimeSpan.FromSeconds(1 / s_context.hertz);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Color.FromNonPremultiplied(50,50,50,255));

            if (s_sample != null)
            {
                _renderer!.Begin(_camera, false, false, false);
                var m = Box2dMeshDrawer.Mesh;
                _renderer.Draw(Matrix4x4.Identity, Box2dMeshDrawer.Mesh);
                _renderer.End();
            }
        }

        private void SwitchSample(int sampleIndex)
        {
            s_context.sampleIndex = sampleIndex;

            // #todo restore all drawing settings that may have been overridden by a sample
            s_context.subStepCount = 4;
            s_context.drawJoints = true;

            // todo testing always using bounds
            s_context.useCameraBounds = true;

            s_sample = null;
            s_sample = g_sampleEntries[s_context.sampleIndex].createFcn(s_context);

            var cameraSettings = s_sample.InitialCameraSettings;
            _camera.FarPlane = 20;
            _camera.NearPlane = 0;
            _camera.Position = cameraSettings.Center.ToVector3(10);
            _camera.LookAt(cameraSettings.Center.ToVector3(0), Vector3.UnitY);
            _camera.Origin = OriginPosition.Center;
            _camera.ViewHeight = cameraSettings.Zoom * 2;
        }
    }
}
