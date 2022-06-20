

using System.Runtime.InteropServices;

namespace WinFormsApp2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ForzaHorizonAPI
    {
        public bool IsRaceOn;
        public uint TimestampMS; // Can overflow to 0 eventually
        public float EngineMaxRpm;
        public float EngineIdleRpm;
        public float CurrentEngineRpm;
        Forza_Vector Acceleration;
        Forza_Vector Velocity;
        Forza_Vector AngularVelocity;
        public float Yaw;
        public float Pitch;
        public float Roll;
        Forza_Suspension SuspensionTravel; // Suspension travel normalized: 0.0f = max stretch; 1.0 = max compression
        Forza_Suspension TireSlipRatio; // Tire normalized slip ratio, = 0 means 100% grip and |ratio| > 1.0 means loss of grip.
        Forza_Suspension WheelRotationSpeed; // Wheel rotation speed radians/sec.
        Forza_Suspension WheelOnRumb; // = 1 when wheel is on rumble strip, = 0 when off.
        Forza_Suspension WheelInPuddleDepth; // = from 0 to 1, where 1 is the deepest puddle
        Forza_Suspension SurfaceRumble; // Non-dimensional surface rumble values passed to controller force feedback
        Forza_Suspension TyreSlipAngle; // Tire normalized slip angle, = 0 means 100% grip and |angle| > 1.0 means loss of grip.
        Forza_Suspension TireCombinedSlip; // Tire normalized combined slip, = 0 means 100% grip and |slip| > 1.0 means loss of grip.
        Forza_Suspension SuspensionTravelInMeter;  // Actual suspension travel in meters
        public uint CarOrdinal; // Unique ID of the car make/model
        public uint CarClass; // Between 0 (D -- worst cars) and 7 (X class -- best cars) inclusive
        public uint CarPerformanceIndex; // Between 100 (slowest car) and 999 (fastest car) inclusive
        public uint DrivetrainType; // Corresponds to EDrivetrainType; 0 = FWD, 1 = RWD, 2 = AWD
        public uint NumCylinders; // Number of cylinders in the engine

        public float empty0;
        public float empty1;
        public float empty2;
        // Dash
        Forza_Vector Position;
        public float Speed;
        public float Power;
        public float Torque;
        Forza_Suspension TireTemp;
        public float Boost;
        //public float Fuel;
        //public float Distance;
        //public float BestLapTime;
        //public float LastLapTime;
        //public float CurrentLapTime;
        //public float CurrentRaceTime;
        //public uint Lap;
        //public uint RacePosition;
        //public uint Throttle;
        //public uint Brake;
        //public uint Clutch;
        //public uint Handbrake;
        //public uint Gear;
        //public int Steer;
        //public uint NormalDrivingLine;
        //public uint NormalAiBrakeDifference;

        Forza_empty test;

        public byte Throttle;
        public byte Brake;
        public byte Clutch;
        public byte Handbrake;
        public byte Gear;
        public byte Steer;


        


    }

    struct Forza_empty
    {
        public byte empty1;
        public byte empty2;
        public byte empty3;
        public byte empty4;
        public byte empty5;
        public byte empty6;
        public byte empty7;
        public byte empty8;
        public byte empty9;
        public byte empty10;
        public byte empty11;
        public byte empty12;
        public byte empty13;
        public byte empty14;
        public byte empty15;
        public byte empty16;
        public byte empty17;
        public byte empty18;
        public byte empty19;
        public byte empty20;
        public byte empty21;
        public byte empty22;
        public byte empty23;
        public byte empty24;
        public byte empty25;
        public byte empty26;
        public byte empty27;
        //public byte empty28;
        //public byte empty29;
        //public byte empty30;
        //public byte empty31;
        //public byte empty32;
        //public byte empty33;
        //public byte empty34;
        //public byte empty35;
        //public byte empty36;
        //public byte empty37;
        //public byte empty38;
        //public byte empty39;
        //public byte empty40;
    }

    struct Forza_Vector
    {
        public float X;
        public float Y;
        public float Z;
    }

    struct Forza_Suspension
    {
        public float FrontLeft;
        public float FrontRight;
        public float RearLeft;
        public float RearRight;
    }
}
