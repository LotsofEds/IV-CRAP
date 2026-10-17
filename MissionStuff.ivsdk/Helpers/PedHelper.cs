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
    internal class PedHelper
    {
        public static Dictionary<UIntPtr, int> PedHandles { get; private set; } = new Dictionary<UIntPtr, int>();

        public static void GrabAllPeds()
        {
            PedHandles.Clear();

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);

                if (ptr != UIntPtr.Zero)
                {
                    int pedHandle = (int)pedPool.GetIndex(ptr);
                    PedHandles[ptr] = pedHandle;
                }
            }
        }
        public static int SpawnPed(int type, string model, Vector3 pos, float heading)
        {
            int thisPed;

            CREATE_CHAR(type, GET_HASH_KEY(model), pos.X, pos.Y, pos.Z, out thisPed, true);
            SET_CHAR_HEADING(thisPed, heading);

            return thisPed;
        }
    }
}
