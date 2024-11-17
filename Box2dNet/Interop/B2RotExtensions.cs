using System.Runtime.InteropServices;

namespace Box2dNet.Interop
{
    public partial struct b2WorldDef
    {
        public b2WorldDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2WorldId
    {
        public override string ToString()
        {
            return $"w[{index1}:{revision}]";
        }
    }

    public partial struct b2BodyDef
    {
        public b2BodyDef()
        {
            throw new Exception("Don't use the constructor of Def structs. Use the B2Api.b2Default~Def() method instead.");
        }
    }

    public partial struct b2BodyId
    {
        public override string ToString()
        {
            return $"w[{world0}].B[{index1}:{revision}]";
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

    public partial struct b2ShapeId
    {
        public override string ToString()
        {
            return $"W[{world0}].S[{index1}:{revision}]";
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
            return new b2Rot { c = MathF.Cos(angle), s = MathF.Sin(angle) };
        }
    }
}
