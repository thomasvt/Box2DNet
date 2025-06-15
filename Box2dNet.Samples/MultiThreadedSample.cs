using System.Diagnostics;
using Box2dNet.Interop;
using System.Numerics;

namespace Box2dNet.Samples
{
    internal class MultiThreadedSample : ISample
    {
        private const int BallCount = 10000;

        public void Run()
        {
            
            Console.WriteLine($"We will now drop {BallCount} balls on the floor: first in a single-threaded, then a multi-threaded test. ");
            Console.WriteLine("We measure time, and track the position of one ball to compare if both tests have the same outcome.");
            Console.WriteLine();
            Console.WriteLine("Press enter to begin ...");
            Console.ReadLine();

            Console.WriteLine("Single threaded test:");
            var sw = Stopwatch.StartNew();
            RunTest(false);
            Console.WriteLine($"Single threaded test took: {sw.Elapsed.TotalSeconds} s");
            Console.WriteLine();

            Console.WriteLine("Multi threaded test:");
            sw = Stopwatch.StartNew();
            RunTest(true);
            Console.WriteLine($"Multi threaded test took: {sw.Elapsed.TotalSeconds} s");
            Console.WriteLine();
        }

        private void RunTest(bool useMultiThreading)
        {
            // create world
            var worldDef = useMultiThreading
                ? B2Api.b2DefaultWorldDef_WithDotNetTpl() // <---- this is all it takes for default multi threading
                : B2Api.b2DefaultWorldDef();

            worldDef.gravity = new(0, -9.81f);
            var b2WorldId = B2Api.b2CreateWorld(worldDef);

            try
            {
                // add a static ground for the blocks to fall onto:
                AddGround(b2WorldId);

                // add a boatload of balls
                for (var i = 0; i < BallCount; i++)
                {
                    AddBall(b2WorldId, new(i % 100, 150 - i / 100));
                }

                // add a block we will be watching while simulating:
                var watchedBlockBodyId = AddBall(b2WorldId, new(50.5f, 50.5f));

                // simulate world a few steps and prove it's alive:
                for (var i = 0; i < 10 * 60; i++) // 10 seconds of simulation
                {
                    B2Api.b2World_Step(b2WorldId, 1 / 60f, 8);

                    if (i % 60 == 0)
                    {
                        var position = B2Api.b2Body_GetPosition(watchedBlockBodyId);
                        var velocity = B2Api.b2Body_GetLinearVelocity(watchedBlockBodyId);
                        Console.WriteLine($"Sim-time = {i / 60f:0.0s}, body position = {position}, velocity = {velocity}");
                    }
                }
            }
            finally
            {
                B2Api.b2DestroyWorld(b2WorldId);
            }
        }

        private static b2BodyId AddBall(b2WorldId b2WorldId, Vector2 position)
        {
            var bodyDef = B2Api.b2DefaultBodyDef();
            bodyDef.type = b2BodyType.b2_dynamicBody;
            bodyDef.position = position;
            var b2BodyId = B2Api.b2CreateBody(b2WorldId, bodyDef);

            var shapeDef = B2Api.b2DefaultShapeDef();
            var circle = new b2Circle { radius = 0.5f };
            B2Api.b2CreateCircleShape(b2BodyId, in shapeDef, in circle);
            return b2BodyId;
        }

        private static void AddGround(b2WorldId b2WorldId)
        {
            var bodyDef = B2Api.b2DefaultBodyDef();
            bodyDef.type = b2BodyType.b2_staticBody;
            var b2BodyId = B2Api.b2CreateBody(b2WorldId, bodyDef);

            var shapeDef = B2Api.b2DefaultShapeDef();
            var polygon = CreatePolygon(new Vector2[] { new(-100, 0), new(-100, 1), new(200, 1), new(200, 0) });
            B2Api.b2CreatePolygonShape(b2BodyId, in shapeDef, in polygon);
        }

        private static b2Polygon CreatePolygon(Vector2[] corners)
        {
            if (corners.Length is < 3 or > 8) throw new Exception($"Corner count ({corners.Length}) must be within [3,8].");

            return B2Api.b2MakePolygon(B2Api.b2ComputeHull(corners, corners.Length), 0);
        }
    }
}

