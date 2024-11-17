using Box2dNet.Interop;
using System.Numerics;

namespace Box2dNet.Samples
{
    internal class PolygonShapeSample : ISample
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

                // ... with polygon shape
                var shapeDef = B2Api.b2DefaultShapeDef();
                var polygon = CreatePolygon(new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) });
                var b2ShapeId = B2Api.b2CreatePolygonShape(b2BodyId, in shapeDef, in polygon);

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

        private static b2Polygon CreatePolygon(Vector2[] corners)
        {
            if (corners.Length is < 3 or > 8) throw new Exception($"Corner count ({corners.Length}) must be within [3,8].");
            
            return B2Api.b2MakePolygon(B2Api.b2ComputeHull(corners, corners.Length), 0);
        }
    }
}
