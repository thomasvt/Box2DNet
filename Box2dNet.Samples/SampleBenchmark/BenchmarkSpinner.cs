using System.Numerics;
using System.Runtime.InteropServices;
using Box2dNet.Interop;

namespace Box2dNet.Samples.SampleBenchmark
{
    internal class BenchmarkSpinner : Sample
    {
        public BenchmarkSpinner(SampleContext context) : base(context)
        {
            // b2_toiCalls = 0;
            // b2_toiHitCount = 0;

            CreateSpinner(m_worldId);
        }

        public override CameraSettings InitialCameraSettings => new(new(0.0f, 32.0f), 42f);

        public override void Step(Box2dMeshDrawer meshDrawer)
        {
            base.Step(meshDrawer);

            if (m_stepCount == 1000 && false)
            {
                // 0.1 : 46544, 25752
                // 0.25 : 5745, 1947
                // 0.5 : 2197, 660
                m_context.pause = true;
            }

            // DrawTextLine( "toi calls, hits = %d, %d", b2_toiCalls, b2_toiHitCount );
        }

        public static Sample Create(SampleContext context)
        {
            return new BenchmarkSpinner(context);
        }

        private const int SPINNER_POINT_COUNT = 360;

        struct SpinnerData
        {
            public b2JointId spinnerId;
        }

        SpinnerData g_spinnerData;

        void CreateSpinner(b2WorldId worldId)
        {
            b2BodyId groundId;
            {
                var bodyDef = B2Api.b2DefaultBodyDef();
                groundId = B2Api.b2CreateBody(worldId, bodyDef);

                var points = new Vector2[SPINNER_POINT_COUNT];

                var q = Matrix3x2.CreateRotation(-2.0f * B2Api.B2_PI / SPINNER_POINT_COUNT);
                Vector2 p = new(40.0f, 0.0f);
                for (int i = 0; i < SPINNER_POINT_COUNT; ++i)
                {
                    points[i] = new(p.X, p.Y + 32.0f);
                    p = Vector2.Transform(p, q);
                }

                b2SurfaceMaterial material = default;
                material.friction = 0.1f;
                var materialHandle = GCHandle.Alloc(material, GCHandleType.Pinned);
                var pointsHandle = GCHandle.Alloc(points, GCHandleType.Pinned);
                try
                {
                    b2ChainDef chainDef = B2Api.b2DefaultChainDef();
                    chainDef.points = pointsHandle.AddrOfPinnedObject();
                    chainDef.count = SPINNER_POINT_COUNT;
                    chainDef.isLoop = true;
                    chainDef.materials = materialHandle.AddrOfPinnedObject();
                    chainDef.materialCount = 1;
                    B2Api.b2CreateChain(groundId, chainDef);
                }
                finally
                {
                    materialHandle.Free();
                    pointsHandle.Free();
                }
            }

            {
                b2BodyDef bodyDef = B2Api.b2DefaultBodyDef();
                bodyDef.type = b2BodyType.b2_dynamicBody;
                bodyDef.position = new(0.0f, 12.0f);
                bodyDef.enableSleep = false;

                b2BodyId spinnerId = B2Api.b2CreateBody(worldId, bodyDef);

                b2Polygon box = B2Api.b2MakeRoundedBox(0.4f, 20.0f, 0.2f);
                b2ShapeDef shapeDef = B2Api.b2DefaultShapeDef();
                shapeDef.material.friction = 0.0f;
                B2Api.b2CreatePolygonShape(spinnerId, shapeDef, box);

                float motorSpeed = 5.0f;
                float maxMotorTorque = 40000.0f;
                b2RevoluteJointDef jointDef = B2Api.b2DefaultRevoluteJointDef();
                jointDef.@base.bodyIdA = groundId;
                jointDef.@base.bodyIdB = spinnerId;
                jointDef.@base.localFrameA.p = bodyDef.position;
                jointDef.enableMotor = true;
                jointDef.motorSpeed = motorSpeed;
                jointDef.maxMotorTorque = maxMotorTorque;

                g_spinnerData.spinnerId = B2Api.b2CreateRevoluteJoint(worldId, jointDef);
            }

            {
                b2Capsule capsule = new b2Capsule(new(-0.25f, 0.0f), new(0.25f, 0.0f), 0.25f);
                b2Circle circle = new b2Circle(new(0.0f, 0.0f), 0.35f);
                b2Polygon square = B2Api.b2MakeSquare(0.35f);

                b2BodyDef bodyDef = B2Api.b2DefaultBodyDef();
                bodyDef.type = b2BodyType.b2_dynamicBody;
                b2ShapeDef shapeDef = B2Api.b2DefaultShapeDef();
                shapeDef.material.friction = 0.1f;
                shapeDef.material.restitution = 0.1f;
                shapeDef.density = 0.25f;

                int bodyCount = 3038;
#if DEBUG
                bodyCount = 499;
#endif

                float x = -24.0f, y = 2.0f;
                for (int i = 0; i < bodyCount; ++i)
                {
                    bodyDef.position = new(x, y);

                    b2BodyId bodyId = B2Api.b2CreateBody(worldId, bodyDef);

                    int remainder = i % 3;
                    if (remainder == 0)
                    {
                        B2Api.b2CreateCapsuleShape(bodyId, shapeDef, capsule);
                    }
                    else if (remainder == 1)
                    {
                        B2Api.b2CreateCircleShape(bodyId, shapeDef, circle);
                    }
                    else if (remainder == 2)
                    {
                        B2Api.b2CreatePolygonShape(bodyId, shapeDef, square);
                    }

                    x += 1.0f;

                    if (x > 24.0f)
                    {
                        x = -24.0f;
                        y += 1.0f;
                    }
                }
            }
        }

        float StepSpinner(b2WorldId worldId, int stepCount)
        {
            return B2Api.b2RevoluteJoint_GetAngle(g_spinnerData.spinnerId);
        }
    }
}
