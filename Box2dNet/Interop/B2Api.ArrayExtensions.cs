using System.Numerics;

namespace Box2dNet.Interop
{
    public partial struct b2ChainDef
    {
        /// <summary>
        /// Allows to read the points directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<Vector2> pointsAsSpan => points.NativeArrayAsSpan<Vector2>(count);

        /// <summary>
        /// Allows to read the materials directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<b2SurfaceMaterial> materialsAsSpan => materials.NativeArrayAsSpan<b2SurfaceMaterial>(materialCount);
    }

    public partial struct b2SensorEvents
    {
        /// <summary>
        /// Allows to read the beginEvents directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<b2SensorBeginTouchEvent> beginEventsAsSpan =>
            beginEvents.NativeArrayAsSpan<b2SensorBeginTouchEvent>(beginCount);

        /// <summary>
        /// Allows to read the endEvents directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<b2SensorEndTouchEvent> endEventsAsSpan =>
            endEvents.NativeArrayAsSpan<b2SensorEndTouchEvent>(endCount);

    }

    public partial struct b2ContactEvents
    {
        /// <summary>
        /// Allows to read the beginEvents directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<b2ContactBeginTouchEvent> beginEventsAsSpan =>
            beginEvents.NativeArrayAsSpan<b2ContactBeginTouchEvent>(beginCount);

        /// <summary>
        /// Allows to read the endEvents directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<b2ContactEndTouchEvent> endEventsAsSpan =>
            endEvents.NativeArrayAsSpan<b2ContactEndTouchEvent>(endCount);

        /// <summary>
        /// Allows to read the hitEvents directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<b2ContactHitEvent> hitEventsAsSpan =>
            hitEvents.NativeArrayAsSpan<b2ContactHitEvent>(hitCount);
    }

    public partial struct b2BodyEvents
    {
        /// <summary>
        /// Allows to read the moveEvents directly from native memory as a span.
        /// </summary>
        public ReadOnlySpan<b2BodyMoveEvent> moveEventsAsSpan =>
            moveEvents.NativeArrayAsSpan<b2BodyMoveEvent>(moveCount);
    }
}
