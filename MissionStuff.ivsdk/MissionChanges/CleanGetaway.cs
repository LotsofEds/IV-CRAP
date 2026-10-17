using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCL.GTAIV;
using IVSDKDotNet;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;
using IVSDKDotNet.Enums;

namespace MissionStuff.ivsdk.MissionChanges
{
    internal class CleanGetaway
    {
        // OtherShit
        private static bool missionStart;
        private static bool driveAway;
        private static int vladCar;
        private static int missionPed;
        private static int driverPed;
        // Lock car door. Spawn one extra guy that drives off with the car. Player has to steal back the car.
        public static void UnInit()
        {
            missionStart = false;
            vladCar = 0;
        }
        public static void Init(SettingsFile settings)
        {
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("vlad3"))
            {
                missionStart = true;
                if (GetMissionCar())
                {
                    SpawnDriverPed();
                    GetPedAggroState();
                    UnlockCarDoors();
                }

                if (driveAway)
                {
                    StealCarAndDriveAway();
                    ProcessFail();
                }
            }
            else if (missionStart)
                UnInit();
        }
        private static bool GetMissionCar()
        {
            if (!DOES_VEHICLE_EXIST(vladCar))
            {
                foreach (var veh in VehHelper.VehHandles)
                {
                    int vehHandle = veh.Value;
                    if (!DOES_VEHICLE_EXIST(vehHandle))
                        continue;
                    if (!IS_CAR_A_MISSION_CAR(vehHandle))
                        continue;

                    GET_CAR_MODEL(vehHandle, out uint vModel);
                    if (vModel == GET_HASH_KEY_2("blista"))
                    {
                        if (LOCATE_CAR_3D(vehHandle, 1192.740f, 818.520f, 36.700f, 2.0f, 2.0f, 2.0f, false))
                        {
                            LOCK_CAR_DOORS(vehHandle, 7);
                            SET_ENGINE_HEALTH(vehHandle, 500.0f);
                            vladCar = vehHandle;
                        }
                    }
                }
                return false;
            }
            else
                return true;
        }
        private static bool GetPedAggroState()
        {
            if (!DOES_CHAR_EXIST(missionPed))
            {
                foreach (var ped in PedHelper.PedHandles)
                {
                    int pedHandle = ped.Value;
                    if (!DOES_CHAR_EXIST(pedHandle))
                        continue;
                    if (!IS_PED_A_MISSION_PED(pedHandle))
                        continue;
                    if (pedHandle == Main.PlayerHandle)
                        continue;

                    GET_CHAR_MODEL(pedHandle, out int pedMdl);
                    if (pedMdl == GET_HASH_KEY("m_y_streetblk_02"))
                    {
                        if (LOCATE_CHAR_ANY_MEANS_3D(pedHandle, 1194.137f, 816.3333f, 34.9701f, 2.0f, 2.0f, 2.0f, false))
                            missionPed = pedHandle;
                    }
                }
                return false;
            }
            else
            {
                GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out int pVeh);
                if (IS_PED_IN_COMBAT(missionPed) || IS_CHAR_INJURED(missionPed) ||
                    (IS_CHAR_GETTING_IN_TO_A_CAR(Main.PlayerHandle) && pVeh == vladCar))
                    driveAway = true;
                return true;
            }
        }
        private static void UnlockCarDoors()
        {
            if (IS_CHAR_IN_CAR(Main.PlayerHandle, vladCar))
                LOCK_CAR_DOORS(vladCar, 1);
        }
        private static void SpawnDriverPed()
        {
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_y_streetblk_03")))
                REQUEST_MODEL(GET_HASH_KEY("m_y_streetblk_03"));

            else if (!DOES_CHAR_EXIST(driverPed)) {
                driverPed = PedHelper.SpawnPed(26, "m_y_streetblk_03", new Vector3(1192.780f, 822.370f, 36.125f), 160);
                SET_CHAR_RELATIONSHIP_GROUP(driverPed, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
            }
        }
        private static void StealCarAndDriveAway()
        {
            if (!IS_CHAR_IN_CAR(driverPed, vladCar))
            {
                _TASK_CAR_DRIVE_WANDER(driverPed, vladCar, 15.0f, 2);
                SET_CHAR_KEEP_TASK(driverPed, true);
            }
        }
        private static void ProcessFail()
        {
            if (!LOCATE_CAR_3D(vladCar, Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, 250.0f, 250.0f, 250.0f, false))
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("V3FAIL", "~r~The car has been lost.~s~");
                EXPLODE_CAR(vladCar, true, false);
            }
        }
    }
}
