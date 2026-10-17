using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using IVSDKDotNet.Enums;
using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk.MissionChanges
{
    internal class ParkourSideAct
    {
        // IniShit
        private static bool autoSave;
        private static bool showBlip;
        private static float defaultStamina;
        private static float staminaIncr;
        private static float staminaDrain;
        private static float meleeIncr;

        // BooleShit
        private static bool gameStart;
        private static bool startMission;
        private static bool startCourse;
        private static bool showHelp;
        private static bool hasFailed;
        private static bool passCourse;

        // OtherShit
        private static int courseIndex;
        private static uint fTimer;
        private static uint hTimer;
        private static int currentTime;
        private static int checkPtIndex;
        private static int checkPtMarker;
        private static int checkPtBlip;
        private static int nextCheckPtBlip;
        private static int staminaUpgrades;

        private static SettingsFile courseSettings;

        private static List<ParkourData> courseList = new List<ParkourData>();

        public static void IngameStart()
        {
            gameStart = false;
            passCourse = false;
            hasFailed = false;
        }
        public static void UnInit()
        {
            for (int i = 0; i < courseList.Count; i++)
            {
                for (int p = 0; p < courseList[i].Props.Count; p++)
                {
                    if (DOES_OBJECT_EXIST(courseList[i].Props[p]))
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(courseList[i].Props[p]);
                }
                if (DOES_BLIP_EXIST(courseList[i].Blip))
                    REMOVE_BLIP(courseList[i].Blip);
            }
            if (DOES_BLIP_EXIST(nextCheckPtBlip))
                REMOVE_BLIP(nextCheckPtBlip);
            if (DOES_BLIP_EXIST(checkPtBlip))
                REMOVE_BLIP(checkPtBlip);

            //DELETE_CHECKPOINT(checkPtMarker);

            fTimer = 0;
            startMission = false;
            startCourse = false;
            showHelp = false;
            courseIndex = 0;
            checkPtIndex = 0;
        }
        public static void Init(SettingsFile settings)
        {
            courseSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\ParkourCourses.ini", IVGame.GameStartupPath));
            courseSettings.Load();

            string[] sectionNames = courseSettings.GetSectionNames();

            for (int i = 0; i < sectionNames.Count(); i++)
            {
                ParkourData newCourse = new ParkourData();

                newCourse.Location = courseSettings.GetVector3(sectionNames[i], "StartLocation", Vector3.Zero);
                newCourse.Heading = courseSettings.GetFloat(sectionNames[i], "StartHeading", 0);
                newCourse.TimeLimit = courseSettings.GetInteger(sectionNames[i], "TimeLimit", 0);

                newCourse.Checkpoints = new List<Vector3>();
                newCourse.Props = new List<int>();
                newCourse.PropModels = new List<string>();
                newCourse.PropLocs = new List<Vector3>();
                newCourse.PropRots = new List<Vector3>();

                newCourse.Blip = -1;

                string[] keys = courseSettings.GetValueNames(sectionNames[i]);

                for (int c = 0; c < keys.Count(); c++)
                {
                    if (keys[c].Contains("Checkpoint"))
                    {
                        Vector3 checkpoint = courseSettings.GetVector3(sectionNames[i], keys[c], Vector3.Zero);
                        newCourse.Checkpoints.Add(checkpoint);
                    }

                    else if (keys[c].Contains("PropModel"))
                    {
                        string propMdl = courseSettings.GetValue(sectionNames[i], keys[c], "ec_nf_ghostball");
                        newCourse.PropModels.Add(propMdl);
                        newCourse.Props.Add(-1);
                    }

                    else if (keys[c].Contains("PropLocation"))
                    {
                        Vector3 propLoc = courseSettings.GetVector3(sectionNames[i], keys[c], Vector3.Zero);
                        newCourse.PropLocs.Add(propLoc);
                    }

                    else if (keys[c].Contains("PropRotation"))
                    {
                        Vector3 propRot = courseSettings.GetVector3(sectionNames[i], keys[c], Vector3.Zero);
                        newCourse.PropRots.Add(propRot);
                    }
                }
                for (int c = 0; c < newCourse.PropModels.Count(); c++)
                {
                    if (newCourse.PropModels.Count() > newCourse.PropLocs.Count())
                        newCourse.PropLocs.Add(Vector3.Zero);

                    if (newCourse.PropModels.Count() > newCourse.PropRots.Count())
                        newCourse.PropRots.Add(Vector3.Zero);
                }
                newCourse.Complete = false;

                courseList.Add(newCourse);
            }

            autoSave = settings.GetBoolean("PARKOUR COURSES", "AutosaveAfterCompletion", false);
            showBlip = settings.GetBoolean("PARKOUR COURSES", "ShowBlips", false);
            defaultStamina = settings.GetFloat("PARKOUR COURSES", "DefaultStamina", 600);
            staminaIncr = settings.GetFloat("PARKOUR COURSES", "StaminaIncrease", 20);
            staminaDrain = settings.GetFloat("PARKOUR COURSES", "StaminaDrainUpgrade", 0.5f);
            meleeIncr = settings.GetFloat("PARKOUR COURSES", "MeleeStaminaIncrease", 5.0f);
        }
        public static void LoadSavedData()
        {
            for (int i = 0; i < courseList.Count; i++)
            {
                if (DOES_BLIP_EXIST(courseList[i].Blip))
                    REMOVE_BLIP(courseList[i].Blip);

                if (!Main.savefileSettings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "ParkourCourse" + i.ToString() + "Complete"))
                    Main.savefileSettings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "ParkourCourse" + i.ToString() + "Complete");

                courseList[i].Complete = Main.savefileSettings.GetBoolean(IVGenericGameStorage.ValidSaveName, "ParkourCourse" + i.ToString() + "Complete", false);
            }
        }
        public static void SaveData()
        {
            int coursesCompleted = 0;
            for (int i = 0; i < courseList.Count; i++)
            {
                if (!Main.savefileSettings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "ParkourCourse" + i.ToString() + "Complete"))
                    Main.savefileSettings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "ParkourCourse" + i.ToString() + "Complete");

                Main.savefileSettings.SetBoolean(IVGenericGameStorage.ValidSaveName, "ParkourCourse" + i.ToString() + "Complete", courseList[i].Complete);
                if (courseList[i].Complete)
                    coursesCompleted++;
            }

            if (!Main.savefileSettings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "MeleeStaminaAdd"))
                Main.savefileSettings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "MeleeStaminaAdd");

            Main.savefileSettings.SetFloat(IVGenericGameStorage.ValidSaveName, "MeleeStaminaAdd", coursesCompleted * meleeIncr);
        }
        public static void Tick()
        {
            if (!gameStart)
            {
                LoadSavedData();
                UnInit();
                gameStart = true;
            }

            ProcessStamina();
            ProcessBlips();
            ProcessMissionStart();
            if (startMission && !startCourse)
            {
                if (SpawnCourseProps())
                    StartCourse();
                ProcessCount();
            }
            else
            {
                RequestScripts();
                ProcessTimer();
                ProcessFail();
                ProcessCourse();
            }
            if (showHelp)
                ShowHelp();
        }
        private static void RequestScripts()
        {
            if (!HAS_SCRIPT_LOADED("roman11"))
                REQUEST_SCRIPT("roman11");
            if (!HAS_SCRIPT_LOADED("cia4"))
                REQUEST_SCRIPT("cia4");
        }
        private static void ProcessStamina()
        {
            staminaUpgrades = 0;
            for (int i = 0; i < courseList.Count; i++)
            {
                if (courseList[i].Complete)
                    staminaUpgrades++;
            }

            if (Main.PlayerPed.PlayerInfo.Stamina > -150)
                Main.PlayerPed.PlayerInfo.Stamina += Main.frameTime * staminaDrain * staminaUpgrades;

            if (Main.PlayerPed.PlayerInfo.Stamina > defaultStamina + staminaIncr * staminaUpgrades)
                Main.PlayerPed.PlayerInfo.Stamina = defaultStamina + staminaIncr * staminaUpgrades;
        }
        private static void ProcessBlips()
        {
            if (!showBlip)
                return;

            for (int i = 0; i < courseList.Count; i++)
            {
                if (!DOES_BLIP_EXIST(courseList[i].Blip))
                {
                    NativeBlip newBlip = NativeBlip.AddBlip(courseList[i].Location);

                    newBlip.Icon = BlipIcon.Misc_Trophy;
                    newBlip.Name = "Parkour";
                    newBlip.ShowOnlyWhenNear = true;

                    courseList[i].Blip = newBlip.Handle;
                }
                else
                {
                    bool isOnIsland = false;

                    uint mapArea = GET_MAP_AREA_FROM_COORDS(courseList[i].Location);
                    int mapUnlock = GET_INT_STAT(363);
                    if (mapArea <= mapUnlock)
                        isOnIsland = true;

                    if (GET_MISSION_FLAG() || !isOnIsland)
                        CHANGE_BLIP_DISPLAY(courseList[i].Blip, (uint)eBlipDisplay.BLIP_DISPLAY_HIDDEN);
                    else
                        CHANGE_BLIP_DISPLAY(courseList[i].Blip, (uint)eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP);
                }
            }
        }
        private static void ProcessMissionStart()
        {
            bool atStartLoc = false;
            if (!GET_MISSION_FLAG())
            {
                for (int i = 0; i < courseList.Count; i++)
                {
                    Vector3 startLoc = courseList[i].Location;

                    if (LOCATE_CHAR_ON_FOOT_3D(Main.PlayerHandle, startLoc.X, startLoc.Y, startLoc.Z, 1.0f, 1.0f, 1.0f, true))
                    {
                        atStartLoc = true;
                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_6_4"))
                        {
                            IVText.TheIVText.ReplaceTextOfTextLabel("TM_6_4", "~s~Press ~INPUT_PICKUP~ to start the parkour challenge.");
                            PRINT_HELP_FOREVER("TM_6_4");
                        }

                        if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.Action))
                            StartParkour(i);

                        break;
                    }
                }
            }
            if (!atStartLoc && IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_6_4"))
                CLEAR_HELP();
        }
        private static void StartParkour(int index)
        {
            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
            SET_MISSION_FLAG(true);
            DO_SCREEN_FADE_OUT(1000);
            checkPtIndex = 0;
            fTimer = 0;
            courseIndex = index;
            startMission = true;
        }
        private static bool SpawnCourseProps()
        {
            if (IS_SCREEN_FADED_OUT())
            {
                bool hasSpawned = true;
                for (int i = 0; i < courseList[courseIndex].PropModels.Count; i++)
                {
                    string propMdl = courseList[courseIndex].PropModels[i];
                    if (!HAS_MODEL_LOADED(GET_HASH_KEY(propMdl)))
                    {
                        hasSpawned = false;
                        REQUEST_MODEL(GET_HASH_KEY(propMdl));
                    }
                    else
                    {
                        int propID = courseList[courseIndex].Props[i];
                        if (!DOES_OBJECT_EXIST(propID))
                        {
                            hasSpawned = false;
                            Vector3 propLoc = courseList[courseIndex].PropLocs[i];
                            Vector3 propRot = courseList[courseIndex].PropRots[i];

                            CREATE_OBJECT_NO_OFFSET(GET_HASH_KEY(propMdl), propLoc, out int newObj, true);
                            SET_OBJECT_ROTATION(newObj, propRot);
                            SET_OBJECT_DYNAMIC(newObj, false);
                            FREEZE_OBJECT_POSITION(newObj, true);
                            SET_OBJECT_COLLISION(newObj, true);

                            courseList[courseIndex].Props[i] = newObj;
                        }
                    }
                }
                return hasSpawned;
            }
            else
                return false;
        }
        private static void StartCourse()
        {
            if (fTimer == 0)
            {
                SET_CHAR_COORDINATES(Main.PlayerHandle, courseList[courseIndex].Location);
                SET_CHAR_HEADING(Main.PlayerHandle, courseList[courseIndex].Heading);
                Main.PlayerPed.PlayerInfo.Stamina = defaultStamina + staminaIncr * staminaUpgrades;
                GET_GAME_TIMER(out fTimer);
            }

            else if (Main.gTimer >= fTimer + 1000 && IS_SCREEN_FADED_OUT())
            {
                SET_GAME_CAM_PITCH(0.0f);
                SET_GAME_CAM_HEADING(0.0f);
                SET_CAM_BEHIND_PED(Main.PlayerHandle);
                DO_SCREEN_FADE_IN(1000);
            }
        }
        private static void ProcessCount()
        {
            if (IS_SCREEN_FADING_IN())
                GET_GAME_TIMER(out fTimer);

            else if (IS_SCREEN_FADED_IN())
            {
                if (!IS_FONT_LOADED(4))
                    LOAD_TEXT_FONT(4);

                int currTime = (int)((fTimer + 4000) - Main.gTimer);

                if (currTime > 0)
                {
                    if (!IS_THIS_PRINT_BEING_DISPLAYED("TM_2_25", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_25", "~s~You must complete the course before the time runs out.");
                        PRINT_NOW("TM_2_25", 5000, true);
                    }

                    SET_TEXT_SCALE(0.5f, 1.0f);
                    SET_TEXT_FONT(4);
                    SET_TEXT_EDGE(true, 10, 10, 10, 5);
                    SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
                    SET_TEXT_CENTRE(true);

                    if (currTime > 1000)
                    {
                        SET_TEXT_COLOUR(153, 69, 69, 255);
                        DISPLAY_TEXT_WITH_NUMBER(0.5f, 0.5f, "NUMBER", (int)Math.Ceiling((double)(currTime - 1000) / 1000));
                    }
                    else
                    {
                        SET_TEXT_COLOUR(87, 124, 88, 255);
                        DISPLAY_TEXT(0.5f, 0.5f, "GO");
                    }
                }
                else
                {
                    SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                    GET_GAME_TIMER(out fTimer);
                    startCourse = true;
                }
            }
        }
        private static void ProcessCourse()
        {
            if (!startCourse)
                return;

            List<Vector3> checkpoints = courseList[courseIndex].Checkpoints;

            if (!DOES_BLIP_EXIST(checkPtBlip))
            {
                if (checkPtIndex < checkpoints.Count - 1)
                {
                    ADD_BLIP_FOR_COORD(checkpoints[checkPtIndex + 1].X, checkpoints[checkPtIndex + 1].Y, checkpoints[checkPtIndex + 1].Z, out int nextBlip);

                    NativeBlip secBlip = new NativeBlip(nextBlip);
                    secBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;
                    secBlip.ShowOnlyWhenNear = false;
                    secBlip.Scale = 0.8f;
                    nextCheckPtBlip = nextBlip;

                    /*checkPtMarker = CREATE_CHECKPOINT(2, checkpoints[checkPtIndex].X, checkpoints[checkPtIndex].Y, checkpoints[checkPtIndex].Z + 1.0f,
                       checkpoints[checkPtIndex + 1].X, checkpoints[checkPtIndex + 1].Y, checkpoints[checkPtIndex + 1].Z + 1.0f, 0.25f); */
                }
                /*else
                    checkPtMarker = CREATE_CHECKPOINT(3, checkpoints[checkPtIndex].X, checkpoints[checkPtIndex].Y, checkpoints[checkPtIndex].Z + 1.0f,
                        checkpoints[checkPtIndex + 1].X, checkpoints[checkPtIndex + 1].Y, checkpoints[checkPtIndex + 1].Z + 1.0f, 0.25f);*/

                ADD_BLIP_FOR_COORD(checkpoints[checkPtIndex].X, checkpoints[checkPtIndex].Y, checkpoints[checkPtIndex].Z, out int newBlip);

                NativeBlip currBlip = new NativeBlip(newBlip);
                currBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;

                if (checkPtIndex == checkpoints.Count - 1)
                    CHANGE_BLIP_SPRITE(newBlip, (uint)BlipIcon.Misc_FinishLine);

                currBlip.ShowOnlyWhenNear = false;
                checkPtBlip = newBlip;
            }

            if (LOCATE_CHAR_ON_FOOT_3D(Main.PlayerHandle, checkpoints[checkPtIndex].X, checkpoints[checkPtIndex].Y, checkpoints[checkPtIndex].Z, 1.0f, 1.0f, 1.0f, true))
            {
                checkPtIndex++;
                PLAY_AUDIO_EVENT("FRONTEND_GAME_PICKUP_CHECKPOINT");
                //DELETE_CHECKPOINT(checkPtMarker);
                REMOVE_BLIP(checkPtBlip);
                REMOVE_BLIP(nextCheckPtBlip);
                if (checkPtIndex == checkpoints.Count)
                    MissionPass();
            }
        }
        private static void ProcessTimer()
        {
            if (!startCourse)
                return;

            SET_TEXT_SCALE(0.25f, 0.5f);
            SET_TEXT_FONT(0);
            SET_TEXT_BACKGROUND(false);
            SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
            SET_TEXT_EDGE(true, 0, 0, 0, 255);
            SET_TEXT_PROPORTIONAL(true);
            SET_TEXT_CENTRE(true);

            currentTime = (int)((fTimer + (uint)courseList[courseIndex].TimeLimit * 1000) - Main.gTimer);

            int min = currentTime / 1000 / 60 % 100;
            int sec = currentTime / 1000 % 60;

            DISPLAY_TEXT_WITH_NUMBER(0.916f, 0.84f, "NUMBER", min);

            USE_PREVIOUS_FONT_SETTINGS();
            DISPLAY_TEXT(0.9245f, 0.84f, "COLON");

            USE_PREVIOUS_FONT_SETTINGS();
            if (sec < 10)
            {
                DISPLAY_TEXT_WITH_NUMBER(0.9335f, 0.84f, "NUMBER", 0);
                USE_PREVIOUS_FONT_SETTINGS();
                DISPLAY_TEXT_WITH_NUMBER(0.944f, 0.84f, "NUMBER", sec);
            }
            else
                DISPLAY_TEXT_WITH_NUMBER(0.938f, 0.84f, "NUMBER", sec);

            USE_PREVIOUS_FONT_SETTINGS();
            SET_TEXT_SCALE(0.3f, 0.6f);
            DISPLAY_TEXT_WITH_NUMBER(0.9315f, 0.80f, "OUTOF_NUM", courseList[courseIndex].Checkpoints.Count);

            USE_PREVIOUS_FONT_SETTINGS();
            SET_TEXT_SCALE(0.35f, 0.7f);
            DISPLAY_TEXT_WITH_NUMBER((checkPtIndex > 9) ? 0.905f : 0.91f, 0.795f, "NUMBER", checkPtIndex);
        }
        private static void ProcessFail()
        {
            if (!startCourse)
                return;

            if (currentTime <= 0)
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_25", "~r~You failed to complete the course in time.");
                hasFailed = true;
            }
            if (IS_CHAR_DEAD(Main.PlayerHandle) || HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle))
                hasFailed = true;

            if (!hasFailed)
                return;

            FinishMission();
        }
        private static void MissionPass()
        {
            if (!courseList[courseIndex].Complete)
                passCourse = true;

            courseList[courseIndex].Complete = true;
            TRIGGER_MISSION_COMPLETE_AUDIO((int)eMissionCompleteAudio.MISSION_COMPLETE_1);

            FinishMission();
        }
        private static void FinishMission()
        {
            UnInit();
            SET_MISSION_FLAG(false);

            if (!LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, 903.126f, 586.456f, 23.743f, 50, 50, 50, false))
                NativeGame.StartNewScript("roman11", 1024);
            else
                NativeGame.StartNewScript("cia4", 1024);

            GET_GAME_TIMER(out hTimer);

            showHelp = true;
        }
        private static void ShowHelp()
        {
            if (IS_THIS_PRINT_BEING_DISPLAYED("R11000", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) || IS_THIS_PRINT_BEING_DISPLAYED("GO_TO_HELI", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
            {
                if (!HAS_DEATHARREST_EXECUTED())
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_25", "~r~You failed to complete the course in time.");

                CLEAR_PRINTS();
                CLEAR_BRIEF();
                if (hasFailed)
                    PRINT_NOW("TM_2_25", 6000, true);
            }
            if (Main.gTimer >= hTimer + 1000)
            {
                if (NativeGame.IsScriptRunning("roman11"))
                    NativeGame.TerminateScriptsWithThisName("roman11");
                if (NativeGame.IsScriptRunning("cia4"))
                    NativeGame.TerminateScriptsWithThisName("cia4");

                if (Main.gTimer >= hTimer + 2000 && passCourse)
                {
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_25", "~s~Stamina upgraded - you can now sprint for longer and recover stamina faster.");
                    PRINT_HELP("TM_2_25");

                    showHelp = false;
                    passCourse = false;

                    if (autoSave)
                        NativeGame.DoAutoSave();
                }
                hasFailed = false;
            }
        }
    }
}
