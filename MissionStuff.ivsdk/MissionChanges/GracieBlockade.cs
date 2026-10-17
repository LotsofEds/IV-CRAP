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
    internal class GracieBlockade
    {
        // IniShit
        private static string blockadeCarModel1;
        private static string blockadeCarModel2;
        private static string pursuitCarModel1;
        private static string pursuitCarModel2;
        private static string pursuitCarModel3;

        // OtherShit
        private static bool missionStart;
        private static bool printHelp;

        private static int[] blockadeCars;
        private static int[] blockadePeds;
        private static int[] pursuitCars;
        private static int[] pursuitPeds;
        private static uint fTimer;
        
        // Add more enemy car spawns near booth tunnel entrance and exit.
        public static void UnInit()
        {
            missionStart = false;
            printHelp = false;

            fTimer = 0;

            for (int i = 0; i < 6; i++)
            {
                if (DOES_CHAR_EXIST(blockadePeds[i]))
                    MARK_CHAR_AS_NO_LONGER_NEEDED(blockadePeds[i]);

                if (DOES_CHAR_EXIST(pursuitPeds[i]))
                    MARK_CHAR_AS_NO_LONGER_NEEDED(pursuitPeds[i]);
            }
            for (int i = 0; i < 3; i++)
            {
                if (DOES_VEHICLE_EXIST(blockadeCars[i]))
                    MARK_CHAR_AS_NO_LONGER_NEEDED(blockadeCars[i]);

                if (DOES_VEHICLE_EXIST(pursuitCars[i]))
                    MARK_CHAR_AS_NO_LONGER_NEEDED(pursuitCars[i]);
            }
        }
        public static void Init(SettingsFile settings)
        {
            missionStart = false;
            blockadeCars = new int[3];
            blockadePeds = new int[6];
            pursuitCars = new int[3];
            pursuitPeds = new int[6];

            blockadeCarModel1 = settings.GetValue("GRACEFUL DETOUR", "BlockadeCarA", "benson");
            blockadeCarModel2 = settings.GetValue("GRACEFUL DETOUR", "BlockadeCarB", "pmp600");
            pursuitCarModel1 = settings.GetValue("GRACEFUL DETOUR", "PursuitCarA", "sentinel");
            pursuitCarModel2 = settings.GetValue("GRACEFUL DETOUR", "PursuitCarB", "pmp600");
            pursuitCarModel3 = settings.GetValue("GRACEFUL DETOUR", "PursuitCarC", "feltzer");
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("gerry4"))
            {
                if (!missionStart && IS_THIS_PRINT_BEING_DISPLAYED("gerry6_04", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    missionStart = true;

                else if (missionStart)
                {
                    PrintObjectives();
                    if (RequestModels())
                        if (SpawnCars())
                            SpawnBlockadePeds();
                }
            }
            else if (missionStart)
            {
                UnInit();
            }
        }
        private static void PrintObjectives()
        {
            if (!printHelp && IS_CHAR_IN_AREA_3D(Main.PlayerHandle, -900.0f, 1150.0f, 17.0f, -775.0f, 1185.0f, 25.0f, false))
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_27", "~s~This bridge is blocked off, find a different route or risk driving through.");
                PRINT_NOW("TM_2_27", 6000, true);
                GET_GAME_TIMER(out fTimer);
                printHelp = true;
            }
            else if (Main.gTimer <= fTimer + 6000 && printHelp)
            {
                PRINT_NOW("TM_2_27", 1000, false);
            }
        }
        private static bool RequestModels()
        {
            if (!HAS_MODEL_LOADED(GET_HASH_KEY(blockadeCarModel1)))
                REQUEST_MODEL(GET_HASH_KEY(blockadeCarModel1));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(blockadeCarModel2)))
                REQUEST_MODEL(GET_HASH_KEY(blockadeCarModel2));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(pursuitCarModel1)))
                REQUEST_MODEL(GET_HASH_KEY(pursuitCarModel1));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(pursuitCarModel2)))
                REQUEST_MODEL(GET_HASH_KEY(pursuitCarModel2));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(pursuitCarModel3)))
                REQUEST_MODEL(GET_HASH_KEY(pursuitCarModel3));
            else
                return true;

            return false;
        }
        private static bool SpawnCars()
        {
            if (!DOES_VEHICLE_EXIST(blockadeCars[0]))
                blockadeCars[0] = VehHelper.SpawnCar(blockadeCarModel1, new Vector3(-743.732f, 1172.072f, 19.376f), 180, true);
            else if (!DOES_VEHICLE_EXIST(blockadeCars[1]))
                blockadeCars[1] = VehHelper.SpawnCar(blockadeCarModel1, new Vector3(-743.732f, 1162.133f, 19.376f), 0, true);
            else if (!DOES_VEHICLE_EXIST(blockadeCars[2]))
                blockadeCars[2] = VehHelper.SpawnCar(blockadeCarModel2, new Vector3(-747.732f, 1167.1025f, 19.376f), 0, true);

            else if (!DOES_VEHICLE_EXIST(pursuitCars[0]))
                pursuitCars[0] = VehHelper.SpawnCar(pursuitCarModel1, new Vector3(-945.634f, 861.963f, 12.808f), 240, true);
            else if (!DOES_VEHICLE_EXIST(pursuitCars[1]))
                pursuitCars[1] = VehHelper.SpawnCar(pursuitCarModel2, new Vector3(-959.663f, 763.478f, 3.925f), 0, true);
            else if (!DOES_VEHICLE_EXIST(pursuitCars[2]))
                pursuitCars[2] = VehHelper.SpawnCar(pursuitCarModel3, new Vector3(-384.738f, 1093.254f, 14.224f), 270, true);
            else
                return true;

            return false;
        }
        private static void SpawnBlockadePeds()
        {
            if (HAS_MODEL_LOADED(GET_HASH_KEY("m_y_gmaf_lo_02")))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (!DOES_CHAR_EXIST(blockadePeds[i]))
                    {
                        switch (i)
                        {
                            case 0:
                                blockadePeds[i] = PedHelper.SpawnPed((int)ePedType.PED_TYPE_RUSSIAN_MOB, "m_y_gmaf_lo_02", new Vector3(-746.732f, 1173.872f, 19.376f), 90);
                                break;
                            case 1:
                                blockadePeds[i] = PedHelper.SpawnPed((int)ePedType.PED_TYPE_RUSSIAN_MOB, "m_y_gmaf_lo_02", new Vector3(-746.732f, 1172.872f, 19.376f), 90);
                                break;
                            case 2:
                                blockadePeds[i] = PedHelper.SpawnPed((int)ePedType.PED_TYPE_RUSSIAN_MOB, "m_y_gmaf_lo_02", new Vector3(-746.732f, 1161.333f, 19.376f), 90);
                                break;
                            case 3:
                                blockadePeds[i] = PedHelper.SpawnPed((int)ePedType.PED_TYPE_RUSSIAN_MOB, "m_y_gmaf_lo_02", new Vector3(-746.732f, 1160.333f, 19.376f), 90);
                                break;
                            case 4:
                                blockadePeds[i] = PedHelper.SpawnPed((int)ePedType.PED_TYPE_RUSSIAN_MOB, "m_y_gmaf_lo_02", new Vector3(-746.732f, 1171.372f, 19.376f), 90);
                                break;
                            case 5:
                                blockadePeds[i] = PedHelper.SpawnPed((int)ePedType.PED_TYPE_RUSSIAN_MOB, "m_y_gmaf_lo_02", new Vector3(-746.732f, 1162.833f, 19.376f), 90);
                                break;
                        }
                        SET_CHAR_WILL_USE_COVER(blockadePeds[i], false);
                        SET_CHAR_WILL_USE_CARS_IN_COMBAT(blockadePeds[i], false);
                        GIVE_WEAPON_TO_CHAR(blockadePeds[i], 15, 99999, true);
                        SET_CHAR_ACCURACY(blockadePeds[i], 80);
                        SET_CHAR_SHOOT_RATE(blockadePeds[i], 100);
                        _TASK_SET_IGNORE_WEAPON_RANGE_FLAG(blockadePeds[i], true);
                        SET_SENSE_RANGE(blockadePeds[i], 100f);
                        ADD_ARMOUR_TO_CHAR(blockadePeds[i], 100);
                        SET_CHAR_RELATIONSHIP(blockadePeds[i], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                    }
                    if (!DOES_CHAR_EXIST(pursuitPeds[i]))
                    {
                        if (i <= 2)
                            CREATE_CHAR_INSIDE_CAR(pursuitCars[i], (int)ePedType.PED_TYPE_RUSSIAN_MOB, GET_HASH_KEY_2("m_y_gmaf_lo_02"), out pursuitPeds[i]);
                        else
                            CREATE_CHAR_AS_PASSENGER(pursuitCars[i - 3], (int)ePedType.PED_TYPE_RUSSIAN_MOB, GET_HASH_KEY_2("m_y_gmaf_lo_02"), 0, out pursuitPeds[i]);
                        
                        SET_CHAR_WILL_DO_DRIVEBYS(pursuitPeds[i], true);
                        SET_CHAR_WILL_USE_CARS_IN_COMBAT(pursuitPeds[i], true);
                        GIVE_WEAPON_TO_CHAR(pursuitPeds[i], 7, 99999, true);
                        SET_SENSE_RANGE(pursuitPeds[i], 100f);
                        SET_CHAR_RELATIONSHIP(pursuitPeds[i], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                    }
                }
            }
            else
                REQUEST_MODEL(GET_HASH_KEY("m_y_gmaf_lo_02"));
        }
    }
}
