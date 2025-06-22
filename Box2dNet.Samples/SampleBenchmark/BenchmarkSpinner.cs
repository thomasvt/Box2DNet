using System.Numerics;
using Box2dNet.Interop;

namespace Box2dNet.Samples.SampleBenchmark
{
    internal class BenchmarkSpinner : Sample
    {
        public BenchmarkSpinner(SampleContext context) : base(context)
        {
            if (m_context.restart == false)
            {
                m_context.camera.m_center = new(0.0f, 32.0f);
                m_context.camera.m_zoom = 42.0f;
            }

            // b2_toiCalls = 0;
            // b2_toiHitCount = 0;

            CreateSpinner(m_worldId);
        }

        void Step()
        {
            base.Step();

            if (m_stepCount == 1000 && false)
            {
                // 0.1 : 46544, 25752
                // 0.25 : 5745, 1947
                // 0.5 : 2197, 660
                m_context.pause = true;
            }

            // DrawTextLine( "toi calls, hits = %d, %d", b2_toiCalls, b2_toiHitCount );
        }

        static Sample Create(SampleContext context)
        {
            return new BenchmarkSpinner(context);
        }

        private const int SPINNER_POINT_COUNT = 360;

        struct SpinnerData
        {
            b2JointId spinnerId;
        }

        SpinnerData g_spinnerData;

        void CreateSpinner(b2WorldId worldId)
        {
            b2BodyId groundId;
            {
                b2BodyDef bodyDef = B2Api.b2DefaultBodyDef();
                groundId = B2Api.b2CreateBody(worldId, &bodyDef);

                Vector2[] points = new Vector2[SPINNER_POINT_COUNT];

                b2Rot q = B2Api.b2MakeRot(-2.0f * B2_PI / SPINNER_POINT_COUNT);
                b2Vec2 p = { 40.0f, 0.0f };
                for (int i = 0; i < SPINNER_POINT_COUNT; ++i)
                {
                    points[i] = (b2Vec2){ p.x, p.y + 32.0f }
                    ;
                    p = b2RotateVector(q, p);
                }

                b2SurfaceMaterial material = { 0 };
                material.friction = 0.1f;

                b2ChainDef chainDef = b2DefaultChainDef();
                chainDef.points = points;
                chainDef.count = SPINNER_POINT_COUNT;
                chainDef.isLoop = true;
                chainDef.materials = &material;
                chainDef.materialCount = 1;

                b2CreateChain(groundId, &chainDef);
            }

            {
                b2BodyDef bodyDef = b2DefaultBodyDef();
                bodyDef.type = b2_dynamicBody;
                bodyDef.position = (b2Vec2){ 0.0, 12.0f }
                ;
                bodyDef.enableSleep = false;

                b2BodyId spinnerId = b2CreateBody(worldId, &bodyDef);

                b2Polygon box = b2MakeRoundedBox(0.4f, 20.0f, 0.2f);
                b2ShapeDef shapeDef = b2DefaultShapeDef();
                shapeDef.material.friction = 0.0f;
                b2CreatePolygonShape(spinnerId, &shapeDef, &box);

                float motorSpeed = 5.0f;
                float maxMotorTorque = 40000.0f;
                b2RevoluteJointDef jointDef = b2DefaultRevoluteJointDef();
                jointDef.base.bodyIdA = groundId;
                jointDef.base.bodyIdB = spinnerId;
                jointDef.base.localFrameA.p = bodyDef.position;
                jointDef.enableMotor = true;
                jointDef.motorSpeed = motorSpeed;
                jointDef.maxMotorTorque = maxMotorTorque;

                g_spinnerData.spinnerId = b2CreateRevoluteJoint(worldId, &jointDef);
            }

            b2Capsule capsule = { { -0.25f, 0.0f }, { 0.25f, 0.0f }, 0.25f };
            b2Circle circle = { { 0.0f, 0.0f }, 0.35f };
            b2Polygon square = b2MakeSquare(0.35f);

            b2BodyDef bodyDef = b2DefaultBodyDef();
            bodyDef.type = b2_dynamicBody;
            b2ShapeDef shapeDef = b2DefaultShapeDef();
            shapeDef.material.friction = 0.1f;
            shapeDef.material.restitution = 0.1f;
            shapeDef.density = 0.25f;

            int bodyCount = BENCHMARK_DEBUG ? 499 : 3038;

            float x = -24.0f, y = 2.0f;
            for (int i = 0; i < bodyCount; ++i)
            {
                bodyDef.position = (b2Vec2){ x, y }
                ;
                b2BodyId bodyId = b2CreateBody(worldId, &bodyDef);

                int remainder = i % 3;
                if (remainder == 0)
                {
                    b2CreateCapsuleShape(bodyId, &shapeDef, &capsule);
                }
                else if (remainder == 1)
                {
                    b2CreateCircleShape(bodyId, &shapeDef, &circle);
                }
                else if (remainder == 2)
                {
                    b2CreatePolygonShape(bodyId, &shapeDef, &square);
                }

                x += 1.0f;

                if (x > 24.0f)
                {
                    x = -24.0f;
                    y += 1.0f;
                }
            }
        }

        float StepSpinner(b2WorldId worldId, int stepCount)
        {
            (void)worldId;
            (void)stepCount;

            return b2RevoluteJoint_GetAngle(g_spinnerData.spinnerId);
        }
    }
}
