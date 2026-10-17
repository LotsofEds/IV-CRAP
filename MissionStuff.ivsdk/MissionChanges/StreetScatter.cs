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
    internal class StreetScatter
    {
        // IniShit
        private static string enemyCarMdl1;
        private static string enemyCarMdl2;

        // OtherShit
        private static bool missionStart;
        private static bool combatStart;
        private static bool driveStart;
        private static List<int> pedList = new List<int>();
        private static int[] myCars;
        private static int[] myPeds;
        private static int[] myBlips;

        // Put peds in car, make em drive away. Player has to manage not losing any peds at the same time.
        public static void UnInit()
        {
            missionStart = false;
            combatStart = false;
            driveStart = false;

            for (int i = 0; i < 2; i++)
            {
                if (DOES_BLIP_EXIST(myBlips[i]))
                    REMOVE_BLIP(myBlips[i]);

                if (DOES_CHAR_EXIST(myPeds[i]))
                    MARK_CHAR_AS_NO_LONGER_NEEDED(myPeds[i]);

                if (DOES_VEHICLE_EXIST(myCars[i]))
                    MARK_CAR_AS_NO_LONGER_NEEDED(myCars[i]);
            }
        }
        public static void Init(SettingsFile settings)
        {
            myCars = new int[2];
            myPeds = new int[2];
            myBlips = new int[2];

            enemyCarMdl1 = settings.GetValue("STREET SCATTER", "CarModelA", "futo");
            enemyCarMdl2 = settings.GetValue("STREET SCATTER", "CarModelB", "fortune");
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("manny2"))
            {
                if (!missionStart && IS_THIS_PRINT_BEING_DISPLAYED("INTDRV_07", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    missionStart = true;

                if (missionStart)
                {
                    if (IS_THIS_PRINT_BEING_DISPLAYED("INTDRV_10", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                        combatStart = true;

                    if (RequestModelsLoaded())
                    {
                        if (CreateCars())
                        {
                            GetMissionPeds();
                            CreatePeds();
                            ProcessPeds();
                            ProcessFleeingPeds();
                        }
                    }
                }
            }
            else if (missionStart)
                UnInit();
        }
        private static bool RequestModelsLoaded()
        {
            if (!HAS_MODEL_LOADED(GET_HASH_KEY(enemyCarMdl1)))
                REQUEST_MODEL(GET_HASH_KEY(enemyCarMdl1));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(enemyCarMdl2)))
                REQUEST_MODEL(GET_HASH_KEY(enemyCarMdl2));
            else
                return true;

            return false;
        }

        private static bool CreateCars()
        {
            if (!DOES_VEHICLE_EXIST(myCars[0]))
                myCars[0] = VehHelper.SpawnCar(enemyCarMdl1, new Vector3(699.104f, 1498.815f, 14.246f), 140, true);
            if (!DOES_VEHICLE_EXIST(myCars[1]))
                myCars[1] = VehHelper.SpawnCar(enemyCarMdl2, new Vector3(704.597f, 1504.227f, 14.327f), 180, true);
            else
                return true;

            return false;
        }

        private static void CreatePeds()
        {
            if (!DOES_CHAR_EXIST(myPeds[0]))
                CREATE_CHAR_INSIDE_CAR(myCars[0], 26, GET_HASH_KEY_2("m_y_glat_lo_01"), out myPeds[0]);
            if (!DOES_CHAR_EXIST(myPeds[1]))
                CREATE_CHAR_INSIDE_CAR(myCars[1], 26, GET_HASH_KEY_2("m_y_glat_lo_01"), out myPeds[1]);
        }
        private static void GetMissionPeds()
        {
            foreach (var ped in PedHelper.PedHandles)
            {
                int pedHandle = ped.Value;
                if (!DOES_CHAR_EXIST(pedHandle))
                    continue;
                if (pedList.Contains(pedHandle))
                    continue;
                if (!IS_PED_A_MISSION_PED(pedHandle))
                    continue;
                if (pedHandle == Main.PlayerHandle)
                    continue;
                if (pedList.Count >= 2)
                    continue;
                if (IS_CHAR_IN_ANY_CAR(pedHandle))
                    continue;
                if (pedHandle == myPeds[0])
                    continue;
                if (pedHandle == myPeds[1])
                    continue;

                pedList.Add(pedHandle);
            }
        }
        private static void ProcessPeds()
        {
            for (int i = 0; i < pedList.Count; i++)
            {
                if (!DOES_CHAR_EXIST(pedList[i]))
                    pedList.RemoveAt(i);
                else
                {
                    SET_CHAR_COLLISION(pedList[i], false);
                    SET_CHAR_VISIBLE(pedList[i], false);
                    CANCEL_CURRENTLY_PLAYING_AMBIENT_SPEECH(pedList[i]);
                    SET_CHAR_INVINCIBLE(pedList[i], true);
                    SET_CURRENT_CHAR_WEAPON(pedList[i], 0, true);
                    _TASK_PLAY_ANIM_NON_INTERRUPTABLE(pedList[i], "idle", "move_player", 8.0f, 0, 0, 1, 0, -1);

                    if (DOES_CHAR_EXIST(myPeds[i]))
                    {
                        GET_CHAR_COORDINATES(myPeds[i], out Vector3 pedPos);
                        _SET_CHAR_COORDINATES_NO_OFFSET(pedList[i], pedPos.X, pedPos.Y, pedPos.Z - 2.5f);
                        if (IS_CHAR_INJURED(myPeds[i]))
                        {
                            if (DOES_BLIP_EXIST(myBlips[i]))
                                REMOVE_BLIP(myBlips[i]);

                            SET_CHAR_INVINCIBLE(pedList[i], false);
                            SET_CHAR_HEALTH(pedList[i], 0);
                            SET_CHAR_COORDINATES(pedList[i], Vector3.Zero);
                        }
                    }
                }
            }
        }
        private static void ProcessFleeingPeds()
        {
            for (int i = 0; i < 2; i++)
            {
                if (!DOES_CHAR_EXIST(myPeds[i]))
                    continue;

                if (!combatStart)
                {
                    CLEAR_CHAR_TASKS(myPeds[i]);
                    SET_CHAR_WILL_LEAVE_CAR_IN_COMBAT(myPeds[i], false);

                    SET_CHAR_RELATIONSHIP_GROUP(myPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);
                    SET_CHAR_RELATIONSHIP(myPeds[i], (int)eRelationship.RELATIONSHIP_NEUTRAL, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                }
                else if (!driveStart)
                {
                    if (!IS_CHAR_INJURED(myPeds[i]))
                    {
                        ADD_BLIP_FOR_CHAR(myPeds[i], out myBlips[i]);
                        GET_CAR_CHAR_IS_USING(myPeds[i], out int pedCar);
                        SET_CAR_FORWARD_SPEED(pedCar, 2.0f);
                        _TASK_CAR_DRIVE_WANDER(myPeds[i], pedCar, 15.0f, 2);
                    }
                }
            }
            if (combatStart)
                driveStart = true;
        }
    }
}
