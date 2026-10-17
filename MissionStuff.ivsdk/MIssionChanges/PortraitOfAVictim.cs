using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk.MissionChanges
{
    internal class PortraitOfAVictim
    {
        // IniShit
        private static string playerCarModel;
        private static string enemyCarMdl1;
        private static string enemyCarMdl2;
        private static string enemyCarMdl3;
        private static string enemyCarMdl4;
        private static string pursuitCarMdl1;
        private static string pursuitCarMdl2;

        // BooleShit
        private static bool missionStart;
        private static bool combatStart;
        private static bool allEnemiesDead;
        private static bool driveForward;
        private static bool startDrive;
        // OtherShit
        private static int soundID;
        private static int phoneProp;
        private static int cam1;
        private static int interpCam;
        private static int cam2;
        private static int missionStage;
        private static uint fTimer;
        private static int bossPed;
        private static int currAlivePed;
        //private static int driverPed;
        private static List<int> pedList = new List<int>();

        private static int[] myBlips;
        private static int[] myPeds;
        private static bool[] pedGotAway;
        private static int[] myCars;

        private static int[] attackCars;
        private static int[] attackPeds;

        private static int playerCar;
        // All this code is kinda fucking ass, rewrite this some day moron.

        // IDEA 1: Dimayev is in one of the scripted cars in the lot.
        // (Maybe) New help message appears when objective print pops up: ~s~It might be best to scope the place out and find Dimayev first...
        // As soon as the goons get aggro'd, the three script cars drive away, Dimayev is in one of them.
        // New Mission Objective: ~s~Some guys are fleeing the scene. ~r~Dimayev ~s~could be one of them, do not let him get away.
        // If a ped in the scripted car get far enough, they "get away" (Not really, the car despawns but the driver just becomes "invisible" using hacky bs).
        // If the player scoped the place out, they will know to kill Dimayev first.
        // If Dimayev is dead, new objective is printed: ~s~Dimayev is dead, eliminate the rest of ~r~them.
        // The other two cars are "decoys", just there to buy time for Dimayev to get away. Doesn't matter if they get away.

        // After killing the guys in the lot. Player is tasked to leave the area.
        // New Objective: ~s~His guards are dead but Dimayev got away, leave the area.
        // After leaving the area, enemy cars spawn around the player as he's driving away. Dimavey has returned with reinforcements.
        // New Objective: ~s~Dimayev has returned with ~r~reinforcements. ~s~Make him wish he didn't.
        // Player has to kill these guys in cars.

        // The intention is to add some "thought" behind the shoootout.
        // If the player just goes in guns blazing not knowing where Dimayev is then they might get surprised by the cars driving away and the game telling them that Dimayev is in one of the cars.
        // The player can still kill Dimayev without planning beforehand by luck or if they are fast enough.

        // IDEA 2 (If idea 1 is too much of a fucking pain in the ass): Dimayev is not actually in the lot. After shootout,  play cutscene of Niko calling ULP:
        // ULP: What have you got for me?
        // Niko: I don't think he's here.
        // Camera cuts to 3 cars arriving, specifically different models and colors, Dimayev is in one of them. He gets out of car, sees bloodbath with Niko standing there on the phone.
        // Dimayev shocked. Maybe just use ambient dialogue. Niko turns around, sees target. At the same time, one other guy gets out of the target's car, another gets out of the third car. They draw guns.
        // Niko, still on the phone: Nevermind.
        // Dimayev gets back in his car. Niko gets in cover as the two guys shoot at him. Gameplay starts.
        // The two guys will act as fodder while Dimayev, along with one other car, drive away.
        // The two cars drive in opposite directions, Niko has to choose which car to follow using the third car that was left behind.
        // If the player payed attention, they will know which car to follow.
        // Print Objective: ~r~Dimayev ~s~is fleeing in one of the cars. Do not let him get away.
        // If a car gets far enough, they will disappear from the radar and "get away" 
        // If one of the cars "get away":
        // Print Objective: ~r~Eliminate ~s~your target.
        // If CORRECT car followed (target eliminated):
        // Mission passes as normal. Kill the rest of the guys using script.
        // If WRONG car followed or player can't keep up with target, they "get away".
        // Print Fail 1 (Player fails to chase Dimayev): ~r~Dimayev got away, there will be consequences...
        // Print Fail 2 (Wrong target killed): ~r~You killed the goon. Dimayev got away, there will be consequences...
        // After fail message, screen fades to black.
        // A few days later... text pops up (PRINT_BIG).
        // Niko gets a hit put on him by Dimayev and is assassinated. Mission fail lmao, no other way to fail the script sadly (except getting arrested).

        // Like with idea 1, the intention is to make the player think a bit more.
        // The game doesn't tell which of car to follow or that there is even a choice. Player has to use common sense to deduce that he can only follow one car.
        public static void UnInit()
        {
            RELEASE_SOUND_ID(soundID);

            soundID = -1;
            missionStage = -1;
            bossPed = -1;
            currAlivePed = -1;
            fTimer = 0;
            pedList.Clear();

            combatStart = false;
            missionStart = false;
            allEnemiesDead = false;
            driveForward = false;
            startDrive = false;

            for (int i = 0; i < 4; i++)
            {
                if (DOES_CHAR_EXIST(myPeds[i]))
                    MARK_CHAR_AS_NO_LONGER_NEEDED(myPeds[i]);

                if (DOES_VEHICLE_EXIST(myCars[i]))
                    MARK_CAR_AS_NO_LONGER_NEEDED(myCars[i]);

                if (DOES_BLIP_EXIST(myBlips[i]))
                    REMOVE_BLIP(myBlips[i]);

                if (DOES_CHAR_EXIST(attackPeds[i]))
                    MARK_CHAR_AS_NO_LONGER_NEEDED(attackPeds[i]);
                pedGotAway[i] = false;
            }
            for (int i = 0; i < 2; i++)
            {
                if (DOES_VEHICLE_EXIST(attackCars[i]))
                    MARK_CAR_AS_NO_LONGER_NEEDED(attackCars[i]);
            }

            if (DOES_VEHICLE_EXIST(playerCar))
                MARK_CAR_AS_NO_LONGER_NEEDED(playerCar);

            if (DOES_OBJECT_EXIST(phoneProp))
                DELETE_OBJECT(ref phoneProp);
        }
        public static void Init(SettingsFile settings)
        {
            allEnemiesDead = false;
            missionStage = -1;
            myBlips = new int[4];
            myPeds = new int[4];
            pedGotAway = new bool[4];
            myCars = new int[4];

            attackCars = new int[2];
            attackPeds = new int[4];

            pedList.Clear();

            for (int i = 0; i < 4; i++)
            {
                pedGotAway[i] = false;
            }

            playerCarModel = settings.GetValue("PURSUIT OF A KILLER", "PlayerCarModel", "uranus");
            enemyCarMdl1 = settings.GetValue("PURSUIT OF A KILLER", "LotCarSmall", "futo");
            enemyCarMdl2 = settings.GetValue("PURSUIT OF A KILLER", "LotCarMed", "faction");
            enemyCarMdl3 = settings.GetValue("PURSUIT OF A KILLER", "LotCarBigA", "patriot");
            enemyCarMdl4 = settings.GetValue("PURSUIT OF A KILLER", "LotCarBigB", "patriot");
            pursuitCarMdl1 = settings.GetValue("PURSUIT OF A KILLER", "PursuitCar1", "patriot");
            pursuitCarMdl2 = settings.GetValue("PURSUIT OF A KILLER", "PursuitCar2", "schafter");

            if (!IS_THIS_MODEL_A_VEHICLE(GET_HASH_KEY_2(playerCarModel)))
                playerCarModel = "uranus";
            if (!IS_THIS_MODEL_A_VEHICLE(GET_HASH_KEY_2(enemyCarMdl1)))
                enemyCarMdl1 = "futo";
            if (!IS_THIS_MODEL_A_VEHICLE(GET_HASH_KEY_2(enemyCarMdl2)))
                enemyCarMdl2 = "fortune";
            if (!IS_THIS_MODEL_A_VEHICLE(GET_HASH_KEY_2(enemyCarMdl3)))
                enemyCarMdl3 = "patriot";
            if (!IS_THIS_MODEL_A_VEHICLE(GET_HASH_KEY_2(enemyCarMdl4)))
                enemyCarMdl4 = "patriot";
            if (!IS_THIS_MODEL_A_VEHICLE(GET_HASH_KEY_2(pursuitCarMdl1)))
                pursuitCarMdl1 = "patriot";
            if (!IS_THIS_MODEL_A_VEHICLE(GET_HASH_KEY_2(pursuitCarMdl2)))
                pursuitCarMdl2 = "schafter";
        }
        public static void Tick()
        {
            // IDEA 1 Alt
            if (NativeGame.IsScriptRunning("cia2"))
            {
                missionStart = true;

                if (missionStage < 14)
                {
                    if (IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                        GetMissionPeds();

                    if (!combatStart)
                    {
                        if (RequestModels())
                            CreateCars();
                    }
                    else
                    {
                        ProcessBlips();
                        ProcessMissionEnd();
                    }

                    ProcessPedBlipInvisibility();
                    ProcessFleeing();
                    ProcessCutscene();

                    if (allEnemiesDead && missionStage <= 0)
                    {
                        if (pedGotAway[2])
                            missionStage = 1;
                        else
                        {
                            for (int i = 0; i < pedList.Count; i++)
                            {
                                if (DOES_CHAR_EXIST(pedList[i]) && !IS_CHAR_INJURED(pedList[i]))
                                {
                                    SET_CHAR_INVINCIBLE(pedList[i], false);
                                    SET_CHAR_HEALTH(pedList[i], 0);
                                }
                            }
                        }
                    }
                }
            }
            else if (missionStart)
            {
                UnInit();
            }
        }
        public static bool RequestModels()
        {
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_m_gru2_lo_02")))
                REQUEST_MODEL(GET_HASH_KEY("m_m_gru2_lo_02"));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(enemyCarMdl1)))
                REQUEST_MODEL(GET_HASH_KEY(enemyCarMdl1));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(enemyCarMdl2)))
                REQUEST_MODEL(GET_HASH_KEY(enemyCarMdl2));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(enemyCarMdl3)))
                REQUEST_MODEL(GET_HASH_KEY(enemyCarMdl3));
            else if (!HAS_MODEL_LOADED(GET_HASH_KEY(enemyCarMdl4)))
                REQUEST_MODEL(GET_HASH_KEY(enemyCarMdl4));
            else
                return true;

            return false;
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
                if (pedHandle == bossPed)
                    continue;

                GET_CHAR_MODEL(pedHandle, out int pModel);

                if (pModel == GET_HASH_KEY("m_y_grus_hi_02") && pedList.Count < 2)
                {
                    bossPed = pedHandle;
                    pedList.Add(pedHandle);
                }
                else if (IS_CHAR_SITTING_IN_ANY_CAR(pedHandle) && pedList.Count >= 1 && pedList.Count < 2)
                {
                    WARP_CHAR_FROM_CAR_TO_COORD(pedHandle, Vector3.Zero);
                    pedList.Add(pedHandle);
                }
                else if (pModel == GET_HASH_KEY("m_m_gru2_lo_02"))
                {
                    allEnemiesDead = true;
                    if (!IS_CHAR_INJURED(pedHandle))
                    {
                        allEnemiesDead = false;
                        if (!DOES_CHAR_EXIST(currAlivePed) || IS_CHAR_INJURED(currAlivePed))
                            currAlivePed = pedHandle;
                        break;
                    }
                }
            }
        }
        private static void CreateCars()
        {
            if (!DOES_VEHICLE_EXIST(myCars[0]))
                myCars[0] = VehHelper.SpawnCar(enemyCarMdl1, new Vector3(1169.209f, 558.702f, 32.960f), 130, true);
            if (!DOES_VEHICLE_EXIST(myCars[1]))
                myCars[1] = VehHelper.SpawnCar(enemyCarMdl2, new Vector3(1190.949f, 603.779f, 38.288f), 0, true);
            if (!DOES_VEHICLE_EXIST(myCars[2]))
                myCars[2] = VehHelper.SpawnCar(enemyCarMdl3, new Vector3(1178.013f, 586.010f, 38.354f), 270, true);
            if (!DOES_VEHICLE_EXIST(myCars[3]))
                myCars[3] = VehHelper.SpawnCar(enemyCarMdl4, new Vector3(1140.329f, 566.685f, 32.288f), 180, true);

            for (int i = 0; i < 4; i++)
            {
                if (DOES_VEHICLE_EXIST(myCars[i]) && IS_CAR_IN_AIR_PROPER(myCars[i]))
                    SET_CAR_ON_GROUND_PROPERLY(myCars[i]);
            }
        }
        private static void ProcessBlips()
        {
            if (missionStage < 0)
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "~s~Some guys are fleeing the scene. ~r~Dimayev ~s~could be one of them, do not let him get away.");
                PRINT_NOW("TM_2_30", 5000, true);
                missionStage = 0;
            }
            for (int i = 0; i < 4; i++)
            {
                if (missionStage <= 1)
                {
                    if (!DOES_BLIP_EXIST(myBlips[i]) && DOES_CHAR_EXIST(myPeds[i]) && !IS_CHAR_INJURED(myPeds[i]))
                    {
                        ADD_BLIP_FOR_CHAR(myPeds[i], out myBlips[i]);
                        SET_BLIP_AS_FRIENDLY(myBlips[i], false);
                    }
                    else if (DOES_BLIP_EXIST(myBlips[i]) && (!DOES_CHAR_EXIST(myPeds[i]) || IS_CHAR_INJURED(myPeds[i])))
                    {
                        if (DOES_CHAR_EXIST(myPeds[i]) && i == 2)
                        {
                            IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_27", "~s~Dimayev is dead, eliminate the rest of the ~r~attackers.");
                            PRINT_NOW("TM_2_27", 5000, true);
                        }
                        REMOVE_BLIP(myBlips[i]);
                    }
                }
                else
                {
                    if (!DOES_BLIP_EXIST(myBlips[i]) && DOES_CHAR_EXIST(attackPeds[i]) && !IS_CHAR_INJURED(attackPeds[i]))
                    {
                        ADD_BLIP_FOR_CHAR(attackPeds[i], out myBlips[i]);
                        SET_BLIP_AS_FRIENDLY(myBlips[i], false);
                    }
                    else if (DOES_BLIP_EXIST(myBlips[i]) && (!DOES_CHAR_EXIST(attackPeds[i]) || IS_CHAR_INJURED(attackPeds[i])))
                    {
                        REMOVE_BLIP(myBlips[i]);
                    }
                }
            }
        }
        private static void ProcessMissionEnd()
        {
            if (missionStage == 13)
            {
                bool notAllInj = false;
                for (int i = 0; i < 4; i++)
                {
                    if (DOES_CHAR_EXIST(attackPeds[i]) && !IS_CHAR_INJURED(attackPeds[i]))
                    {
                        notAllInj = true;
                        break;
                    }
                }
                if (!notAllInj)
                {
                    for (int i = 0; i < pedList.Count; i++)
                    {
                        if (DOES_CHAR_EXIST(pedList[i]) && !IS_CHAR_INJURED(pedList[i]))
                        {
                            SET_CHAR_INVINCIBLE(pedList[i], false);
                            SET_CHAR_HEALTH(pedList[i], 0);
                            missionStage++;
                        }
                    }
                }
            }
        }
        private static void ProcessPedBlipInvisibility()
        {
            for (int i = 0; i < 4; i++)
            {
                if (DOES_CHAR_EXIST(myPeds[i]) && !IS_CHAR_INJURED(myPeds[i]))
                    currAlivePed = myPeds[i];

                if (DOES_CHAR_EXIST(attackPeds[i]) && !IS_CHAR_INJURED(attackPeds[i]) && missionStage >= 13)
                    currAlivePed = attackPeds[i];
            }
            for (int i = 0; i < pedList.Count(); i++)
            {
                if (!DOES_CHAR_EXIST(pedList[i]))
                    pedList.RemoveAt(i);

                else
                {
                    if (IS_PED_IN_COMBAT(pedList[i]))
                        combatStart = true;

                    if (missionStage != 14)
                    {
                        SET_CHAR_COLLISION(pedList[i], false);
                        SET_CHAR_VISIBLE(pedList[i], false);
                        SET_CHAR_INVINCIBLE(pedList[i], true);
                        CANCEL_CURRENTLY_PLAYING_AMBIENT_SPEECH(pedList[i]);
                        SET_CURRENT_CHAR_WEAPON(pedList[i], 0, true);
                        _TASK_PLAY_ANIM_NON_INTERRUPTABLE(pedList[i], "idle", "move_player", 8.0f, 0, 0, 1, 0, -1);
                    }
                    if (DOES_CHAR_EXIST(currAlivePed))
                    {
                        GET_CHAR_COORDINATES(currAlivePed, out Vector3 pedPos);
                        GET_CHAR_HEADING(currAlivePed, out float pedHdng);

                        _SET_CHAR_COORDINATES_NO_OFFSET(pedList[i], pedPos.X, pedPos.Y, pedPos.Z - 2.5f);
                        SET_CHAR_HEADING(pedList[i], pedHdng);
                    }
                }
            }
        }
        private static void ProcessFleeing()
        {
            if (pedList.Count == 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (!DOES_CHAR_EXIST(myPeds[i]) && DOES_VEHICLE_EXIST(myCars[i]) && !pedGotAway[i])
                    {
                        if (i == 2)
                            CREATE_CHAR_INSIDE_CAR(myCars[i], (int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY_2("m_y_grus_hi_02"), out myPeds[i]);
                        else
                            CREATE_CHAR_INSIDE_CAR(myCars[i], (int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY_2("m_m_gru2_lo_02"), out myPeds[i]);
                        SET_CHAR_RELATIONSHIP_GROUP(myPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);
                        SET_CHAR_WILL_LEAVE_CAR_IN_COMBAT(myPeds[i], false);
                    }
                    else if (combatStart)
                    {
                        if (DOES_CHAR_EXIST(myPeds[i]))
                        {
                            if (!IS_CHAR_INJURED(myPeds[i]) && allEnemiesDead)
                                allEnemiesDead = false;

                            if (IS_CHAR_IN_ANY_CAR(myPeds[i]))
                            {
                                if (i == 3 && LOCATE_CHAR_ANY_MEANS_3D(myPeds[i], 1140.329f, 566.785f, 32.288f, 2.0f, 2.0f, 2.0f, false))
                                    SET_CAR_FORWARD_SPEED(myCars[i], 10);

                                if (i == 2 && LOCATE_CHAR_ANY_MEANS_3D(myPeds[i], 1178.013f, 585.910f, 38.254f, 2.0f, 2.0f, 2.0f, false))
                                    SET_CAR_FORWARD_SPEED(myCars[i], 5);
                                if ((i == 3 || i == 2) && !driveForward)
                                {
                                    _TASK_CAR_TEMP_ACTION(myPeds[i], myCars[0], 9, 3000);
                                    GET_GAME_TIMER(out fTimer);
                                    driveForward = true;
                                }
                            }

                            else if (!HAS_CHAR_GOT_WEAPON(myPeds[i], 12))
                            {
                                CLEAR_CHAR_TASKS(myPeds[i]);
                                SET_CHAR_RELATIONSHIP(myPeds[i], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                                GIVE_WEAPON_TO_CHAR(myPeds[i], 12, 30000, false);
                            }

                            if (!IS_CHAR_INJURED(myPeds[i]) && !LOCATE_CHAR_ANY_MEANS_3D(myPeds[i], Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, 250, 250, 250, false))
                            {
                                if (DOES_CHAR_EXIST(myPeds[i]))
                                    DELETE_CHAR(ref myPeds[i]);
                                if (DOES_VEHICLE_EXIST(myCars[i]))
                                    DELETE_CAR(ref myCars[i]);
                                if (i == 2)
                                {
                                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_27", "~s~Dimayev got away, eliminate the rest of the ~r~attackers.");
                                    PRINT_NOW("TM_2_27", 5000, true);
                                }

                                pedGotAway[i] = true;
                            }
                        }
                    }
                }
            }
            if (combatStart)
            {
                ProcessPedFleeLogic(0, new Vector3(844.088f, 816.143f, 10.124f));
                if (Main.gTimer >= fTimer + 3000)
                {
                    startDrive = false;
                    ProcessPedFleeLogic(1, new Vector3(1424.022f, 948.071f, 13.955f));
                    ProcessPedFleeLogic(2, new Vector3(1095.510f, 277.437f, 31.370f));
                }
                ProcessPedFleeLogic(3, new Vector3(1495.134f, 384.247f, 19.021f));

                // To prevent this cunt from flipping.
                if (DOES_VEHICLE_EXIST(myCars[3]) && IS_CAR_IN_AIR_PROPER(myCars[3]))
                    APPLY_FORCE_TO_CAR(myCars[3], 3, 0, 0, -2 * Main.frameTime, 0, -2, 0, 0, 1, 1, 1);

                startDrive = true;
            }
        }
        private static void ProcessCutscene()
        {
            // Fade out.
            if (missionStage == 1)
            {
                DO_SCREEN_FADE_OUT(1000);
                SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                missionStage++;
            }
            // Init cutscene.
            else if (missionStage == 2 && IS_SCREEN_FADED_OUT())
            {
                _SET_CHAR_COORDINATES_NO_OFFSET(Main.PlayerHandle, 1191.297f, 614.736f, 38.759f);
                SET_CHAR_HEADING(Main.PlayerHandle, 60);

                DISPLAY_RADAR(false);
                DISPLAY_HUD(false);

                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);
                SET_CURRENT_CHAR_WEAPON(Main.PlayerHandle, 0, true);

                missionStage++;
            }
            else if (missionStage == 3)
            {
                REQUEST_MODEL(GET_HASH_KEY(pursuitCarMdl1));
                REQUEST_MODEL(GET_HASH_KEY(pursuitCarMdl2));
                REQUEST_MODEL(GET_HASH_KEY(playerCarModel));
                REQUEST_MODEL(GET_HASH_KEY("cj_mobile_hand_1"));
                if (HAS_MODEL_LOADED(GET_HASH_KEY(pursuitCarMdl1)) && HAS_MODEL_LOADED(GET_HASH_KEY(pursuitCarMdl2))
                    && HAS_MODEL_LOADED(GET_HASH_KEY(playerCarModel)) && HAS_MODEL_LOADED(GET_HASH_KEY("cj_mobile_hand_1")))
                    missionStage++;
            }
            // Player enters spawned car as sirens are heard in the distance.
            else if (missionStage == 4)
            {
                playerCar = VehHelper.SpawnCar(playerCarModel, new Vector3(1181.964f, 618.150f, 38.352f), 270, true);
                LOCK_CAR_DOORS(playerCar, 7);

                CREATE_CAM(14, out cam1);

                SET_CAM_POS(cam1, 1187.783f, 621.475f, 40.648f);
                SET_CAM_FOV(cam1, 45);
                POINT_CAM_AT_VEHICLE(cam1, playerCar);
                SET_CAM_ACTIVE(cam1, true);
                SET_CAM_PROPAGATE(cam1, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);

                _TASK_ENTER_CAR_AS_DRIVER(Main.PlayerHandle, playerCar, 45000);
                SET_CAR_ON_GROUND_PROPERLY(playerCar);

                soundID = GET_SOUND_ID();
                PLAY_SOUND_FROM_POSITION(soundID, "AMBIENT_HORNS_REGULAR_SIRENS_COP_SIREN_LOOP", 744.088f, 616.143f, 10.124f);

                DO_SCREEN_FADE_IN(1000);
                GET_GAME_TIMER(out fTimer);

                missionStage++;
            }
            // As soon as player is in car, teleport somewhere down the road, switch to diff cam angle.
            else if (missionStage == 5 && (Main.gTimer > fTimer + 8000 || IS_CHAR_IN_CAR(Main.PlayerHandle, playerCar)))
            {
                SET_CAM_POS(cam1, 1136.874f, 464.423f, 36.085f);
                SET_CAM_FOV(cam1, 60);
                POINT_CAM_AT_COORD(cam1, 1134.874f, 459.423f, 34.045f);
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);

                SET_CAR_COORDINATES(playerCar, 1140.291f, 470.332f, 28.816f);
                SET_CAR_HEADING(playerCar, 150);
                GET_GAME_TIMER(out fTimer);
                missionStage++;
            }
            // Spawn driver ped cuz player can't drive.
            else if (missionStage == 6)
            {
                SET_CAR_ENGINE_ON(playerCar, true, true);
                SET_CAR_ON_GROUND_PROPERLY(playerCar);
                WARP_CHAR_INTO_CAR(Main.PlayerHandle, playerCar);
                _TASK_CAR_MISSION_COORS_TARGET_NOT_AGAINST_TRAFFIC(Main.PlayerHandle, playerCar, 1095.369f, 295.624f, 31.060f, 4, 10.0f, 1, 5, 10);
                
                SET_CAR_FORWARD_SPEED(playerCar, 10);

                CLEAR_AREA_OF_CARS(1147.378f, 482.162f, 28.608f, 50.0f);

                GET_GAME_TIMER(out fTimer);

                missionStage++;
            }
            // Cam angle switch. Create attackers.
            else if (missionStage == 7 && Main.gTimer > fTimer + 5000)
            {
                STOP_SOUND(soundID);
                RELEASE_SOUND_ID(soundID);
                SET_CAR_COORDINATES(playerCar, 1108.428f, 384.504f, 29.378f);
                SET_CAR_FORWARD_SPEED(playerCar, 10);
                ATTACH_CAM_TO_VEHICLE(cam1, playerCar);
                SET_CAM_FOV(cam1, 60);
                SET_CAM_ATTACH_OFFSET_IS_RELATIVE(cam1, true);
                SET_CAM_ATTACH_OFFSET(cam1, -1.2f, -0.4f, 1.0f);
                POINT_CAM_AT_VEHICLE(cam1, playerCar);
                MARK_CAR_AS_NO_LONGER_NEEDED(playerCar);

                CLEAR_AREA_OF_CARS(1108.428f, 384.504f, 29.378f, 50.0f);

                attackCars[0] = VehHelper.SpawnCar(pursuitCarMdl1, new Vector3(1109.301f, 406.075f, 29.339f), 165, true);
                attackCars[1] = VehHelper.SpawnCar(pursuitCarMdl2, new Vector3(1122.662f, 405.205f, 29.395f), 170, true);

                SET_CAR_ENGINE_ON(attackCars[0], true, true);
                SET_CAR_ENGINE_ON(attackCars[1], true, true);

                SET_CAR_FORWARD_SPEED(attackCars[0], 12);
                SET_CAR_FORWARD_SPEED(attackCars[1], 12);

                CREATE_CHAR_INSIDE_CAR(attackCars[0], (int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY_2("m_m_gru2_lo_02"), out attackPeds[0]);
                SET_CHAR_RELATIONSHIP_GROUP(attackPeds[0], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);

                CREATE_CHAR_AS_PASSENGER(attackCars[0], (int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY_2("m_y_grus_hi_02"), (uint)VehicleSeat.RightFront, out attackPeds[1]);
                SET_CHAR_RELATIONSHIP_GROUP(attackPeds[1], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);

                CREATE_CHAR_INSIDE_CAR(attackCars[1], (int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY_2("m_m_gru2_lo_02"), out attackPeds[2]);
                SET_CHAR_RELATIONSHIP_GROUP(attackPeds[2], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);

                CREATE_CHAR_AS_PASSENGER(attackCars[1], (int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY_2("m_m_gru2_lo_02"), (uint)VehicleSeat.RightFront, out attackPeds[3]);
                SET_CHAR_RELATIONSHIP_GROUP(attackPeds[3], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);

                GIVE_WEAPON_TO_CHAR(attackPeds[0], 12, 40000, true);
                GIVE_WEAPON_TO_CHAR(attackPeds[1], 12, 40000, true);
                GIVE_WEAPON_TO_CHAR(attackPeds[2], 12, 40000, true);
                GIVE_WEAPON_TO_CHAR(attackPeds[3], 12, 40000, true);

                SET_CHAR_WILL_USE_CARS_IN_COMBAT(attackPeds[0], true);
                SET_CHAR_WILL_USE_CARS_IN_COMBAT(attackPeds[1], true);
                SET_CHAR_WILL_USE_CARS_IN_COMBAT(attackPeds[2], true);
                SET_CHAR_WILL_USE_CARS_IN_COMBAT(attackPeds[3], true);

                SET_CHAR_WILL_DO_DRIVEBYS(attackPeds[1], true);
                SET_CHAR_WILL_DO_DRIVEBYS(attackPeds[3], true);

                _TASK_CAR_MISSION_COORS_TARGET_NOT_AGAINST_TRAFFIC(attackPeds[0], attackCars[0], 1095.369f, 295.624f, 31.060f, 4, 20.0f, 1, 5, 10);
                _TASK_CAR_MISSION_COORS_TARGET_NOT_AGAINST_TRAFFIC(attackPeds[2], attackCars[1], 1095.369f, 295.624f, 31.060f, 4, 20.0f, 1, 5, 10);

                REMOVE_CAR_WINDOW(attackCars[0], (int)eVehicleWindow.VEHICLE_WINDOW_RF);
                REMOVE_CAR_WINDOW(attackCars[1], (int)eVehicleWindow.VEHICLE_WINDOW_RF);

                GET_GAME_TIMER(out fTimer);

                missionStage++;
            }
            // Player brings up phone, about to call ULP.
            // Attackers shoot at player car.
            else if (missionStage == 8 && Main.gTimer > fTimer + 500)
            {
                _TASK_SHOOT_AT_CHAR(attackPeds[1], Main.PlayerHandle, 1000, (int)5);

                CREATE_OBJECT_NO_OFFSET(GET_HASH_KEY("cj_mobile_hand_1"), 0, 0, 0, out phoneProp, true);
                ATTACH_OBJECT_TO_PED(phoneProp, Main.PlayerHandle, (int)eBone.BONE_RIGHT_HAND, Vector3.Zero, Vector3.Zero, 1);

                _TASK_PLAY_ANIM_SECONDARY_IN_CAR(Main.PlayerHandle, "cellphone_text", "cellphone", 4.0f, 0, 0, 1, 0, -1);

                GET_GAME_TIMER(out fTimer);
                missionStage++;
            }
            // Cam interps to game cam.
            else if (missionStage == 9 && Main.gTimer > fTimer + 750)
            {
                SET_INTERP_FROM_SCRIPT_TO_GAME(true, 1500);
                ACTIVATE_SCRIPTED_CAMS(false, false);

                GET_GAME_TIMER(out fTimer);
                missionStage++;
            }
            // Player brings down phone.
            else if (missionStage == 10 && Main.gTimer > fTimer + 250)
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_29", "~s~Dimayev has returned with ~r~reinforcements, ~s~make him regret that decision.");
                SAY_AMBIENT_SPEECH(Main.PlayerHandle, "IN_COVER_DODGE_BULLETS", true, false, 0);
                SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "cellphone", "cellphone_text", 0.8f);
                GET_GAME_TIMER(out fTimer);
                missionStage++;
            }
            // Delete phone prop then wait for cam to interp.
            else if (missionStage == 11 && Main.gTimer > fTimer + 500)
            {
                DELETE_OBJECT(ref phoneProp);
                GET_GAME_TIMER(out fTimer);
                missionStage++;
            }
            // Gameplay starts.
            else if (missionStage == 12 && Main.gTimer > fTimer + 750)
            {
                PRINT_NOW("TM_2_29", 5000, true);
                SET_CHAR_RELATIONSHIP(attackPeds[0], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                SET_CHAR_RELATIONSHIP(attackPeds[1], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                SET_CHAR_RELATIONSHIP(attackPeds[2], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                SET_CHAR_RELATIONSHIP(attackPeds[3], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);

                CLEAR_CHAR_TASKS(attackPeds[0]);
                CLEAR_CHAR_TASKS(attackPeds[2]);

                CLEAR_CHAR_TASKS(Main.PlayerHandle);

                DESTROY_ALL_CAMS();
                ACTIVATE_SCRIPTED_CAMS(false, false);
                DISPLAY_RADAR(true);
                DISPLAY_HUD(true);

                SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                missionStage++;
            }
        }
        private static void ProcessPedFleeLogic(int index, Vector3 loc)
        {
            if (DOES_CHAR_EXIST(myPeds[index]) && IS_CHAR_IN_ANY_CAR(myPeds[index]))
            {
                if (LOCATE_CHAR_ANY_MEANS_3D(myPeds[index], 1150.329f, 576.785f, 32.288f, 250.0f, 250.0f, 250.0f, false))
                {
                    if (!startDrive)
                    _TASK_CAR_MISSION_COORS_TARGET(myPeds[index], myCars[index], loc.X, loc.Y, loc.Z, 4, 15.0f, 2, 5, 10);
                }
                else
                    _TASK_CAR_DRIVE_WANDER(myPeds[index], myCars[index], 15.0f, 2);
            }
        }
    }
}
