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
    internal class YesWayOnTheSubway
    {
        private static string bikeModel1;
        private static string bikeModel2;

        private static int[] myBikes;
        private static int[] myPeds;

        private static bool missionStart;
        public static void UnInit()
        {
            missionStart = false;
        }
        public static void Init(SettingsFile settings)
        {
            myBikes = new int[4];
            myPeds = new int[8];
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("ray5"))
            {
                if (!missionStart && IS_THIS_PRINT_BEING_DISPLAYED("RAY5_03", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    missionStart = true;

                if (missionStart)
                {
                    if (RequestModels())
                    {
                        if (SpawnBikesA())
                            SpawnPedsA();
                    }
                }
            }
            else if (missionStart)
                UnInit();
        }
        private static bool RequestModels()
        {
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_y_gbik_lo_01")))
                REQUEST_MODEL(GET_HASH_KEY("m_y_gbik_lo_01"));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_y_gbik_lo_02")))
                REQUEST_MODEL(GET_HASH_KEY("m_y_gbik_lo_02"));

            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(bikeModel1)))
                REQUEST_MODEL(GET_HASH_KEY(bikeModel1));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(bikeModel2)))
                REQUEST_MODEL(GET_HASH_KEY(bikeModel2));
            else
                return true;

            return false;
        }
        private static bool SpawnBikesA()
        {
            if (!DOES_VEHICLE_EXIST(myBikes[0]))
                myBikes[0] = VehHelper.SpawnCar(bikeModel1, Vector3.Zero, 0, true);
            else if (!DOES_VEHICLE_EXIST(myBikes[1]))
                myBikes[1] = VehHelper.SpawnCar(bikeModel2, Vector3.Zero, 0, true);
            else
                return true;

            return false;
        }
        private static void SpawnPedsA()
        {
            for (int i = 0; i < 4; i++)
            {
                if (DOES_CHAR_EXIST(myPeds[i]))
                    continue;

                switch (i)
                {
                    case 0:
                        CREATE_CHAR_INSIDE_CAR(myBikes[0], 26, GET_HASH_KEY_2("m_y_gbik_lo_01"), out myPeds[i]);
                        break;
                    case 1:
                        CREATE_CHAR_INSIDE_CAR(myBikes[1], 26, GET_HASH_KEY_2("m_y_gbik_lo_02"), out myPeds[i]);
                        break;
                    case 2:
                        CREATE_CHAR_AS_PASSENGER(myBikes[0], 26, GET_HASH_KEY_2("m_y_gbik_lo_02"), 0, out myPeds[i]);
                        break;
                    case 3:
                        CREATE_CHAR_AS_PASSENGER(myBikes[1], 26, GET_HASH_KEY_2("m_y_gbik_lo_01"), 0, out myPeds[i]);
                        break;
                }

                SET_PED_FALL_OFF_BIKES_WHEN_SHOT(myPeds[i], false);
                SET_CHAR_SUFFERS_CRITICAL_HITS(myPeds[i], false);
                SET_CHAR_RELATIONSHIP_GROUP(myPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
            }
        }
    }
}
