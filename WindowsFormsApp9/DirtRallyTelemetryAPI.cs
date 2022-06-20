using System.Runtime.InteropServices;

namespace WinFormsApp2
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class DirtRallyTelemetryAPI
    {
		public float TotalTime;
		public float StageTime;
		public float LapDistance;
		public float TotalDistance;
		ThreePoints Position;
		public float Speed;
		ThreePoints Velocity;
		ThreePoints Roll;
		ThreePoints Pitch;
		FourWheel Suspension_Position;
		FourWheel Suspension_Velocity;
		FourWheel Wheel_Speed;
		public float Throttle;
		public float Steer;
		public float Brake;
		public float Clutch;
		public float Gear;
		public float Gforce_lat;
		public float Gforce_lon;
		public float CurrentLap;
		public float EngineRPM;
		public float SliProNativeSupport;
		public float CurrentPosition;
		public float zero1;
		public float zero2;
		public float zero3;
		public float zero4;
		public float zero5;
		public float zero6;
		public float zero7;
		public float zero8;
		public float zero9;
		public float FirstSectorTime;
		public float SecondSectorTime;
		FourWheel BrakeTemps;
		public float zero10;
		public float zero11;
		public float zero12;
		public float zero13;
		public float CurrentLap2;
		public float TotalLaps;
		public float TrackLenght;
		public float LastLapTime;
		public float MaxRPM;

    }

	public struct ThreePoints
    {
		public float x;
		public float y;
		public float z;
	}

	public struct FourWheel
	{
		public float back_left;
		public float back_right;
		public float front_left;
		public float front_right;
	}

}
