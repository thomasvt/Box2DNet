using System.Numerics;

namespace Box2dNet.Interop
{
    public partial struct b2WorldDef
    {
        public b2WorldDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2WorldId : IEquatable<b2WorldId>
    {
        public override string ToString()
        {
            return $"w[{index1}:{generation}]";
        }

        public bool Equals(b2WorldId other)
        {
            return index1 == other.index1 && generation == other.generation;
        }

        public override bool Equals(object? obj)
        {
            return obj is b2WorldId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(index1, generation);
        }

        public static bool operator ==(b2WorldId a, b2WorldId b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(b2WorldId a, b2WorldId b)
        {
            return !(a == b);
        }
    }

    public partial struct b2BodyDef
    {
        public b2BodyDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2BodyId : IEquatable<b2BodyId>
    {
        public override string ToString()
        {
            return $"w[{world0}].B[{index1}:{generation}]";
        }

        public bool Equals(b2BodyId other)
        {
            return index1 == other.index1 && world0 == other.world0 && generation == other.generation;
        }

        public override bool Equals(object? obj)
        {
            return obj is b2BodyId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(index1, world0, generation);
        }

        public static bool operator ==(b2BodyId a, b2BodyId b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(b2BodyId a, b2BodyId b)
        {
            return !(a == b);
        }
    }

    public partial struct b2ChainDef
    {
        public b2ChainDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2ShapeDef
    {
        public b2ShapeDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2ShapeId : IEquatable<b2ShapeId>
    {
        public override string ToString()
        {
            return $"W[{world0}].S[{index1}:{generation}]";
        }

        public bool Equals(b2ShapeId other)
        {
            return index1 == other.index1 && world0 == other.world0 && generation == other.generation;
        }

        public override bool Equals(object? obj)
        {
            return obj is b2ShapeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(index1, world0, generation);
        }

        public static bool operator ==(b2ShapeId a, b2ShapeId b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(b2ShapeId a, b2ShapeId b)
        {
            return !(a == b);
        }
    }

    public partial struct b2WeldJointDef
    {
        public b2WeldJointDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2RevoluteJointDef
    {
        public b2RevoluteJointDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2MouseJointDef
    {
        public b2MouseJointDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2MotorJointDef
    {
        public b2MotorJointDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2WheelJointDef
    {
        public b2WheelJointDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2DistanceJointDef
    {
        public b2DistanceJointDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2ExplosionDef
    {
        public b2ExplosionDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2Rot
    {
        /// <summary>
        /// Gets the angle in radians.
        /// </summary>
        public float GetAngle()
        {
            return MathF.Atan2(s, c);
        }

        /// <summary>
        /// From angle in radians.
        /// </summary>
        public static b2Rot FromAngle(float angle)
        {
            return new b2Rot(MathF.Cos(angle), MathF.Sin(angle));
        }

        /// <summary>
        /// Returns a rotation of zero degrees.
        /// </summary>
        public static b2Rot Zero = b2Rot.FromAngle(0);
    }

    public partial struct b2Transform
    {
        /// <summary>
        /// Returns a transform with zero translation and zero rotation.
        /// </summary>
        public static b2Transform Zero = new b2Transform(Vector2.Zero, b2Rot.Zero);
    }

    public partial struct b2Polygon
    {
        public b2Polygon()
        {
            throw new Exception("DO NOT construct b2Polygon, instead use a helper function like b2MakePolygon or b2MakeBox.");
        }
    }

    public partial struct b2Hull
    {
        public b2Hull()
        {
            throw new Exception("DO NOT construct b2Hull, instead use b2ComputeHull().");
        }
    }
}
