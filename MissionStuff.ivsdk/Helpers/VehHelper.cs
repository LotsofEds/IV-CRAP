using CCL;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

// Credits: ItsClonkAndre
namespace MissionStuff.ivsdk
{
    internal class VehHelper
    {
        public static Dictionary<UIntPtr, int> VehHandles { get; private set; } = new Dictionary<UIntPtr, int>();

        public static void GrabAllVehicles()
        {
            VehHandles.Clear();

            IVPool vehPool = IVPools.GetVehiclePool();
            for (int i = 0; i < vehPool.Count; i++)
            {
                UIntPtr ptr = vehPool.Get(i);

                if (ptr != UIntPtr.Zero)
                {
                    int vehHandle = (int)vehPool.GetIndex(ptr);
                    VehHandles[ptr] = vehHandle;
                }
            }
        }
        public static int SpawnCar(string model, Vector3 pos, float heading, bool setOnGround)
        {
            int thisCar;

            CREATE_CAR(GET_HASH_KEY(model), pos.X, pos.Y, pos.Z, out thisCar, true);
            SET_CAR_HEADING(thisCar, heading);
            if (setOnGround)
                SET_CAR_ON_GROUND_PROPERLY(thisCar);

            return thisCar;
        }
    }
}
