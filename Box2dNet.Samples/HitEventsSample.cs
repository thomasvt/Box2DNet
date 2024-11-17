using Box2dNet.Interop;
using System.Numerics;

namespace Box2dNet.Samples
{
    internal class HitEventsSample : ISample
    {
        public void Run()
        {
            // create world
            var worldDef = B2Api.b2DefaultWorldDef();
            worldDef.gravity = new(0, 0);
            var b2WorldId = B2Api.b2CreateWorld(worldDef);

            try
            {
                var ballBodyId = AddBall(b2WorldId, new(-2, 0), new(5, 0));
                AddBall(b2WorldId, new(2, 0), new(0, 0));

                // simulate world and detect collisions:
                for (var i = 0; i < 40; i++)
                {
                    B2Api.b2World_Step(b2WorldId, 1 / 60f, 4);

                    var position = B2Api.b2Body_GetPosition(ballBodyId);
                    var velocity = B2Api.b2Body_GetLinearVelocity(ballBodyId);
                    Console.WriteLine($"Step {i}: ball position = {position}, velocity = {velocity}");

                    var hitEvents = B2Api.b2World_GetContactEvents(b2WorldId);
                    // use helper extension method to efficiently read the native hitEvents array with little code:
                    foreach (var @event in hitEvents.beginEvents.NativeArrayAsSpan<b2ContactBeginTouchEvent>(hitEvents.beginCount))
                    {
                        Console.WriteLine($"!!!!!!!   HIT detected between {@event.shapeIdA} and {@event.shapeIdB}");
                    }
                }
            }
            finally
            {
                B2Api.b2DestroyWorld(b2WorldId);
            }
        }

        private static b2BodyId AddBall(b2WorldId b2WorldId, Vector2 position, Vector2 velocity)
        {
            var bodyDef = B2Api.b2DefaultBodyDef();
            bodyDef.type = b2BodyType.b2_dynamicBody;
            bodyDef.position = position;
            bodyDef.linearVelocity = velocity;
            var b2BodyId = B2Api.b2CreateBody(b2WorldId, bodyDef);

            var shapeDef = B2Api.b2DefaultShapeDef();
            var circle = new b2Circle { radius = 0.5f };
            B2Api.b2CreateCircleShape(b2BodyId, in shapeDef, in circle);
            return b2BodyId;
        }
    }
}
