using System.Numerics;
using Box2dNet.Interop;

namespace Box2dNet.Samples
{
    internal class DebugDrawSample : ISample
    {
        public void Run()
        {
            Console.WriteLine("This sample implements B2DebugDraw to write a few 'draw' calls to the console and then executes the draw on a simple B2World:\n");

            // create world
            var worldDef = B2Api.b2DefaultWorldDef();
            worldDef.gravity = new(0, -9.81f);
            var b2WorldId = B2Api.b2CreateWorld(worldDef);

            try
            {
                AddBall(b2WorldId, new(-5, 0));
                AddBall(b2WorldId, new(5, 0));

                var debugDraw = ConsoleDebugDraw.Create();

                B2Api.b2World_Draw(b2WorldId, ref debugDraw);
            }
            finally
            {
                B2Api.b2DestroyWorld(b2WorldId);
            }
        }

        

        private static void AddBall(b2WorldId b2WorldId, Vector2 position)
        {
            // add body ...
            var bodyDef = B2Api.b2DefaultBodyDef();
            bodyDef.type = b2BodyType.b2_dynamicBody;
            bodyDef.position = position;
            var b2BodyId = B2Api.b2CreateBody(b2WorldId, bodyDef);

            // ... with circle shape
            var shapeDef = B2Api.b2DefaultShapeDef();
            var circle = new b2Circle() { radius = 1 };
            var b2ShapeId = B2Api.b2CreateCircleShape(b2BodyId, in shapeDef, in circle);
        }
    }
}
