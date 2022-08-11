using System.Runtime.InteropServices;

namespace WinFormsApp2
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class LiveForSpeedTelemetryAPI
    {
        public char L;
        public char F;
        public char S;
        public char T;
        public int ID;
        public uint Time;
		public OutSimMain OSMain;
		public OutSimInputs OSInputs;
		public byte Gear;              // 0=R, 1=N, 2=first gear
		public byte Sp1;               // spare
		public byte Sp2;
		public byte Sp3;
		public float EngineAngVel;     // radians/s
		public float MaxTorqueAtVel;
		public float CurrentLapDist;       // m - travelled by car
		public float IndexedDistance;
        public OutSimWheel OSWheels1;
		public OutSimWheel OSWheels2;
		public OutSimWheel OSWheels3;
		public OutSimWheel OSWheels4; // array of structs - see above
	}


	public struct OutSimMain
	{
		public Vector_LFS AngVel;      // 3 floats, angular velocity vector
		public float Heading;  // anticlockwise from above (Z)
		public float Pitch;        // anticlockwise from right (X)
		public float Roll;     // anticlockwise from front (Y)
		public Vector_LFS Accel;       // 3 floats X, Y, Z
		public Vector_LFS Vel;     // 3 floats X, Y, Z
		public Vector_LFS_int Pos;        // 3 ints   X, Y, Z (1m = 65536)
	};

	public struct OutSimInputs
	{
		public float Throttle;     // 0 to 1
		public float Brake;            // 0 to 1
		public float InputSteer;       // radians
		public float Clutch;           // 0 to 1
		public float Handbrake;        // 0 to 1
	};

	public struct OutSimWheel // 10 ints
	{
		public float SuspDeflect;      // compression from unloaded
		public float Steer;                // including Ackermann and toe
		public float XForce;               // force right
		public float YForce;               // force forward
		public float VerticalLoad;     // perpendicular to surface
		public float AngVel;               // radians/s
		public float LeanRelToRoad;        // radians a-c viewed from rear

		public byte AirTemp;           // degrees C
		public byte SlipFraction;      // (0 to 255 - see below)
		public byte Touching;          // touching ground
		public byte Sp3;

		public float SlipRatio;            // slip ratio
		public float TanSlipAngle;     // tangent of slip angle
	};

	public struct Vector_LFS
    {
		public float X;
		public float Y;
		public float Z;
    }

	public struct Vector_LFS_int
	{
		public int X;
		public int Y;
		public int Z;
	}
}
