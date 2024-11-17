using Box2dNet.Interop;

namespace Box2dNet.Samples
{
    internal class CircleShapeSample : ISample
    {
        public void Run()
        {
            // create world
            var worldDef = B2Api.b2DefaultWorldDef();
            worldDef.gravity = new(0, -9.81f);
            var b2WorldId = B2Api.b2CreateWorld(worldDef);

            try
            {
                // add body ...
                var bodyDef = B2Api.b2DefaultBodyDef();
                bodyDef.type = b2BodyType.b2_dynamicBody;
                var b2BodyId = B2Api.b2CreateBody(b2WorldId, bodyDef);

                // ... with circle shape
                var shapeDef = B2Api.b2DefaultShapeDef();
                var circle = new b2Circle() { radius = 1 };
                var b2ShapeId = B2Api.b2CreateCircleShape(b2BodyId, in shapeDef, in circle);

                // simulate world a few steps and prove it's alive:
                for (var i = 0; i < 10; i++)
                {
                    B2Api.b2World_Step(b2WorldId, 1 / 60f, 4);
                    var position = B2Api.b2Body_GetPosition(b2BodyId);
                    var velocity = B2Api.b2Body_GetLinearVelocity(b2BodyId);
                    Console.WriteLine($"Step {i}: body position = {position}, velocity = {velocity}");
                }
            }
            finally
            {
                B2Api.b2DestroyWorld(b2WorldId);
            }
        }
    }
}
