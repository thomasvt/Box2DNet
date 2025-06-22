using System.Numerics;
using System.Runtime.CompilerServices;

namespace Box2dNet.Samples.Graphics;

public static class GeometryExtensions
{
    /// <summary>
    /// Normalises the vector, or returns Vector2.Zero if the vector has no length.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 NormalizeOrZero(this Vector2 v)
    {
        return v == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ReverseY(this Vector2 v)
    {
        return v with { Y = -v.Y };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToVector3(this Vector2 v, float z = 0)
    {
        return new Vector3(v.X, v.Y, z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Triangle3 ToTriangle3(this Triangle2 t, float z = 0)
    {
        return new Triangle3(new(t.A, z), new(t.B, z), new(t.C, z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 XY(this Vector3 v)
    {
        return new Vector2(v.X, v.Y);
    }

    /// <summary>
    /// Gets a perpendicular vector pointing to the left from the original vector and with the same length. (when X+ is right, Y+ is up)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 CrossLeft(this Vector2 v)
    {
        return new Vector2(-v.Y, v.X);
    }

    /// <summary>
    /// Gets a perpendicular vector pointing to the right from the original vector and with the same length. (when X+ is right, Y+ is up)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 CrossRight(this Vector2 v)
    {
        return new Vector2(v.Y, -v.X);
    }

    /// <summary>
    /// Adds y to the vector's Y value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 TranslateY(this Vector2 v, float y)
    {
        return v with { Y = v.Y + y };
    }

    /// <summary>
    /// Adds x to the vector's X value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 TranslateX(this Vector2 v, float x)
    {
        return v with { X = v.X + x };
    }

    /// <summary>
    /// Calculates the cross product ("a×b"). Interpretable as the surface area of the parallelogram formed by the two vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cross(this Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    /// <summary>
    /// Returns the vector capped to maxLength. If it's shorter, the original vector is returned.
    /// </summary>
    public static Vector2 CapVectorLength(this Vector2 v, float maxLength)
    {
        var length = v.Length();
        if (length > maxLength)
            return v / length * maxLength;
        return v;
    }

    public static Matrix4x4 To4x4(this Matrix3x2 m)
    {
        return new Matrix4x4(
            m.M11, m.M12, 0, 0, 
            m.M21, m.M22, 0, 0, 
            0, 0, 1, 0, 
            m.M31, m.M32, 0, 1);
    }

    public static Vector2 AngleToVector2(this float angle, float length = 1f)
    {
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * length;
    }

    public static float GetAngle(this Vector2 v)
    {
        return MathF.Atan2(v.Y, v.X);
    }

    public static float? GetAngleOrDefault(this Vector2 v, float? @default = null)
    {
        return v is { X: 0, Y: 0 } ? @default : MathF.Atan2(v.Y, v.X);
    }

    /// <summary>
    /// Returns the signed difference between two angles in radians within (-180, 180). It always returns the shortest path.
    /// </summary>
    public static float GetShortestAngleTo(this float fromAngle, float toAngle)
    {
        fromAngle = NormalizeAngle(fromAngle);
        toAngle = NormalizeAngle(toAngle);

        var diff = toAngle - fromAngle;
        var diffAbs = MathF.Abs(diff);
        if (diffAbs > MathF.PI)
        {
            // pick the other, shorter way around the circle:
            diff = -MathF.Sign(diff) * (MathF.Tau - diff);
        }

        return diff;
    }

    /// <summary>
    /// Normalizes an angle in radians to [0, tau) (=2*pi)
    /// </summary>
    public static float NormalizeAngle(this float angle)
    {
        angle %= MathF.Tau;
        if (angle < 0)
            return angle + MathF.Tau;
        return angle;
    }

    /// <summary>
    /// Similar to interpolating from 'vector' to 'target' but with a fixed step-distance, instead of a percentage.
    /// Adds to 'vector' a step-vector pointing from 'vector' to 'target'. Does not overshoot.
    /// </summary>
    /// <param name="isTargetReached">True if 'step' was large enough to reach 'target', or false if not.</param>
    public static Vector2 StepTowards(this Vector2 vector, Vector2 target, float stepDistance, out bool isTargetReached)
    {
        var inc2 = stepDistance * stepDistance;
        var toTarget = target - vector;
        if (toTarget.LengthSquared() <= inc2)
        {
            isTargetReached = true;
            return target;
        }

        isTargetReached = false;
        return vector + Vector2.Normalize(toTarget) * stepDistance;
    }

    /// <summary>
    /// Rotates the angle one step towards the targetAngle along the shortest side of the circle. Guarantees the final step returns 'targetAngle' exactly.
    /// </summary>
    public static float RotateTowards(this float angle, float targetAngle, float absoluteStep, out bool isTargetReached)
    {
        var shortestAngleChange = angle.GetShortestAngleTo(targetAngle);

        if (MathF.Abs(shortestAngleChange) <= absoluteStep)
        {
            isTargetReached = true;
            return targetAngle;
        }

        isTargetReached = false;
        return angle + MathF.Sign(shortestAngleChange) * absoluteStep;
    }

    /// <summary>
    /// Steps the value one step towards 'target'. If the step is larger than the remaining distance to 'target', the result is set to the exact target and true is returned. Else false is returned.
    /// </summary>
    /// <param name="isTargetReached">True if 'step' was large enough to reach 'target', or false if not.</param>
    public static float StepTowards(this float value, float target, float absoluteStep, out bool isTargetReached)
    {
        var toTarget = target - value;
        if (MathF.Abs(toTarget) <= absoluteStep)
        {
            isTargetReached = true;
            return target;
        }

        isTargetReached = false;
        return value + MathF.Sign(toTarget) * absoluteStep;
    }

    public static float ManhattanLength(this Vector2 v)
    {
        return MathF.Abs(v.X) + MathF.Abs(v.Y);
    }
}