using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp9
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct ACCsPageFilePhysics
    {
        public int packetId;
        public float gas;
        public float brake;
        public float fuel;
        public int gear;
        public int rpms;
        public float steerAngle;
        public float speedKmh;
        public Acc_Vector velocity;
        public Acc_Vector accG;
        public Acc_4wheels wheelSlip;
        public Acc_4wheels wheelLoad;

        public Acc_4wheels wheelsPressure;
        public Acc_4wheels wheelAngularSpeed;
        public Acc_4wheels tyreWear;
        public Acc_4wheels tyreDirtyLevel;
        public Acc_4wheels tyreCoreTemperature;
        public Acc_4wheels camberRAD;
        public Acc_4wheels suspensionTravel;
        public float drs;
        public float tc;
        public float heading;
        public float pitch;
        public float roll;
        public float cgHeight;
        public Acc_Car_Damage carDamage;
        public int numberOfTyresOut;
        public int pitLimiterOn;
        public float abs;
        public float kersCharge;
        public float kersInput;
        public int autoShifterOn;
        public float rideHeight_front;
        public float rideHeight_rear;
        public float turboBoost;
        public float ballast;
        public float airDensity;
        public float airTemp;
        public float roadTemp;
        public Acc_Vector localAngularVel;
        public float finalFF;
        public float performanceMeter;
        public int engineBrake;
        public int ersRecoveryLevel;
        public int ersPowerLevel;
        public int ersHeatCharging;
        public int ersIsCharging;
        public float kersCurrentKJ;
        public int drsAvailable;
        public int drsEnabled;
        public Acc_4wheels brakeTemp;
        public float clutch;
        public Acc_4wheels tyreTempI;
        public Acc_4wheels tyreTempM;
        public Acc_4wheels tyreTempO;
        public int isAIControlled;
        public Acc_Tyre_Contact tyreContactPoint;
        public Acc_Tyre_Contact tyreContactNormal;
        public Acc_Tyre_Contact tyreContactHeading;
        public float brakeBias;
        public Acc_Vector localVelocity;
        public int P2PActivation;
        public int P2PStatus;
        public float currentMaxRpm;
        public Acc_4wheels mz;
        public Acc_4wheels fx;
        public Acc_4wheels fy;
        public Acc_4wheels slipRatio;
        public Acc_4wheels slipAngle;
        public int tcinAction;
        public int absInAction;
        public Acc_4wheels suspensionDamage;
        public Acc_4wheels tyreTemp;
        public float waterTemp;
        public Acc_4wheels brakePressure;
        public int frontBrakeCompound;
        public int rearBrakeCompound;
        public Acc_4wheels padLife;
        public Acc_4wheels discLife;

    }
    public struct Acc_Vector
    {
        public float x;
        public float y;
        public float z;
    }

    public struct Acc_4wheels
    {
        public float front_left;
        public float front_right;
        public float rear_left;
        public float rear_right;
    }
    public struct Acc_Car_Damage
    {
        public float left;
        public float right;
        public float front;
        public float rear;
        public float center;
    }
    public struct Acc_Tyre_Contact
    {
        public Acc_Tyre front_left;
        public Acc_Tyre front_right;
        public Acc_Tyre rear_left;
        public Acc_Tyre rear_rear;
    }
    public struct Acc_Tyre
    {
        public float inside;
        public float midle;
        public float outside;
    }
}
