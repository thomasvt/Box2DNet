using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Box2dNet.Interop
{
    public static partial class B2Api
    {
        /// <summary>
        /// Used with b2World_CastRayClosest_DotNet.
        /// </summary>
        public class CastRayClosestFilter
        {
            public b2QueryFilter QueryFilter { get; }
            public Func<b2ShapeId, bool> Predicate { get; }

            internal b2ShapeId? ShapeId = default;
            internal float Fraction = default;
            internal bool Hit = false;
            internal Vector2 Point = default;
            internal Vector2 Normal = default;

            public CastRayClosestFilter(b2QueryFilter queryFilter, Func<b2ShapeId, bool> predicate)
            {
                QueryFilter = queryFilter;
                Predicate = predicate;
            }

            public float CastCallback(b2ShapeId shapeId, Vector2 point, Vector2 normal, float fraction, IntPtr /* void* */ context)
            {
                if (!Predicate(shapeId)) 
                    return -1; // ignore this hit

                // because we return 'fraction' we only get called here for an even closer hit than we already had.

                // So, store the info about the latest closest hit:
                ShapeId = shapeId; // (over)write the closest shape yet.
                Hit = true;
                Fraction = fraction;
                Point = point;
                Normal = normal;

                return fraction; // only callback again for an even closer shape
            }
        }

        /// <summary> 
        /// .NET extension method similar to b2World_CastRayClosest but with custom filtering.
        /// </summary>
        public static b2RayResult b2World_CastRayClosest_DotNet(b2WorldId worldId, Vector2 origin, Vector2 translation, CastRayClosestFilter filter)
        {
            // clear previous results in case a single instance is reused for many casts to decrease GC pressure
            filter.ShapeId = default;
            filter.Fraction = default;
            filter.Hit = default;
            filter.Point = default;
            filter.Normal = default;

            using var contextHandle = new NativeHandle<CastRayClosestFilter>(filter);
            var callbackIntPtr = Marshal.GetFunctionPointerForDelegate((b2CastResultFcn)filter.CastCallback);
            var stats = b2World_CastRay(worldId, origin, translation, filter.QueryFilter, callbackIntPtr, contextHandle.IntPtr);
            return new b2RayResult
            {
                leafVisits = stats.leafVisits,
                nodeVisits = stats.nodeVisits,
                fraction = filter.Fraction,
                hit = filter.Hit,
                point = filter.Point,
                normal = filter.Normal
            };
        }

        
    }
}
