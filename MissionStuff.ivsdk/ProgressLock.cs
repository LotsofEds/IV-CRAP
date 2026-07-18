using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class ProgressLock
    {
        // IniShit
        public static SettingsFile lockSettings;
        private static int numberOfLocks;

        // OtherShit
        private static bool tpThePlayer;
        private static LockData[] lockList;

        public static void Init(SettingsFile settings)
        {
            lockSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\MissionLocks.ini", IVGame.GameStartupPath));
            lockSettings.Load();

            string[] sections = lockSettings.GetSectionNames();
            numberOfLocks = sections.Count();

            lockList = new LockData[numberOfLocks];

            for (int i = 0; i < numberOfLocks; i++)
            {
                lockList[i] = new LockData();
                GetMissionLock(lockSettings, i);
            }
        }
        private static void GetMissionLock(SettingsFile settings, int lockedMissionIndex)
        {
            if (settings.DoesSectionExists(lockedMissionIndex.ToString()))
            {
                lockList[lockedMissionIndex].MarkerPos = settings.GetVector3(lockedMissionIndex.ToString(), "MissionStartPos", Vector3.Zero);
                lockList[lockedMissionIndex].TpPos = settings.GetVector3(lockedMissionIndex.ToString(), "TeleportAwayCoords", Vector3.Zero);
                lockList[lockedMissionIndex].TpHdng = settings.GetFloat(lockedMissionIndex.ToString(), "TeleportAwayHeading", 0);
                lockList[lockedMissionIndex].TpDist = settings.GetFloat(lockedMissionIndex.ToString(), "MissionDistance", 0);

                lockList[lockedMissionIndex].StatID = settings.GetInteger(lockedMissionIndex.ToString(), "MissionStatID", 0);
                lockList[lockedMissionIndex].StatMin = settings.GetFloat(lockedMissionIndex.ToString(), "MissionMinProgress", 0);
                lockList[lockedMissionIndex].StatMax = settings.GetFloat(lockedMissionIndex.ToString(), "MissionMaxProgress", 0);

                lockList[lockedMissionIndex].UnlockStat = settings.GetInteger(lockedMissionIndex.ToString(), "MissionUnlockStat", 0);
                lockList[lockedMissionIndex].UnlockProg = settings.GetFloat(lockedMissionIndex.ToString(), "MissionUnlockRequirement", 0);
                lockList[lockedMissionIndex].TpMsg = settings.GetValue(lockedMissionIndex.ToString(), "MessageToDisplay", "");
            }
        }
        public static void Tick()
        {
            for (int i = 0; i < numberOfLocks; i++)
            {
                //IVGame.ShowSubtitleMessage(GET_MISSION_FLAG().ToString());
                if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, lockList[i].MarkerPos.X, lockList[i].MarkerPos.Y, lockList[i].MarkerPos.Z, lockList[i].TpDist, lockList[i].TpDist, lockList[i].TpDist, false))
                {
                    if (!GET_MISSION_FLAG() && GET_FLOAT_STAT(lockList[i].StatID) > lockList[i].StatMin && GET_FLOAT_STAT(lockList[i].StatID) < lockList[i].StatMax && !HAS_DEATHARREST_EXECUTED())
                    {
                        if (GET_FLOAT_STAT(lockList[i].UnlockStat) < lockList[i].UnlockProg && !tpThePlayer)
                        {
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                            if (IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
                            {
                                GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out int pVeh);
                                FREEZE_CAR_POSITION(pVeh, true);
                            }
                            else
                                FREEZE_CHAR_POSITION(Main.PlayerHandle, true);

                            DO_SCREEN_FADE_OUT(1000);
                            tpThePlayer = true;
                        }
                    }
                    if (tpThePlayer && IS_SCREEN_FADED_OUT())
                    {
                        if (IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
                        {
                            GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out int pVeh);
                            FREEZE_CAR_POSITION(pVeh, false);
                            SET_CAR_COORDINATES(pVeh, lockList[i].TpPos);
                            SET_CAR_HEADING(pVeh, lockList[i].TpHdng);
                            SET_CAR_ON_GROUND_PROPERLY(pVeh);
                        }
                        else
                        {
                            FREEZE_CHAR_POSITION(Main.PlayerHandle, false);
                            SET_CHAR_COORDINATES(Main.PlayerHandle, lockList[i].TpPos);
                            SET_CHAR_HEADING(Main.PlayerHandle, lockList[i].TpHdng);
                        }
                        DO_SCREEN_FADE_IN(1000);
                        SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_25", lockList[i].TpMsg);
                        PRINT_HELP("TM_2_25");
                        //IVGame.ShowSubtitleMessage(tpMessage, 5000);
                        tpThePlayer = false;
                    }
                }
            }
        }
    }
    public class LockData
    {
        public Vector3 MarkerPos { get; set; }
        public Vector3 TpPos { get; set; }
        public float TpHdng { get; set; }
        public float TpDist { get; set; }
        public int StatID { get; set; }
        public float StatMin { get; set; }
        public float StatMax { get; set; }
        public int UnlockStat { get; set; }
        public float UnlockProg { get; set; }
        public string TpMsg { get; set; }
        public LockData()
        {
        }
    }
}
