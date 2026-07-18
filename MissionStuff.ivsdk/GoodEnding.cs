using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class GoodEnding
    {
        private static readonly List<int> PedList = new List<int>();
        private static bool areCreditsRolling = false;
        private static int cam;
        private static int cam2;
        private static int interpCam;
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("faustin2"))
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
                    if (IS_CHAR_INJURED(pedHandle))
                        continue;
                    if (PedList.Contains(pedHandle))
                        continue;

                    GET_CHAR_MODEL(pedHandle, out int pModel);

                    if (pModel == GET_HASH_KEY("ig_dmitri"))
                        PedList.Add(pedHandle);
                }
            }
            else if (PedList.Count > 0)
            {
                foreach (var ped in PedList)
                {
                    if (!DOES_CHAR_EXIST(ped))
                        continue;

                    if (IS_CHAR_INJURED(ped))
                    {
                        SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                        SET_MAX_WANTED_LEVEL(0);

                        REQUEST_ADDITIONAL_TEXT("CREDIT", 0);
                        REQUEST_ANIMS("amb@dance_femidl_b");

                        START_END_CREDITS_MUSIC();
                        START_CREDITS();
                        SET_CHAR_COORDINATES(Main.PlayerHandle, -605.477f, -749.437f, 93.146f);
                        SET_CHAR_HEADING(Main.PlayerHandle, 0);
                        SET_PLAYER_MOOD_NORMAL(Main.PlayerIndex);

                        if (!DOES_CAM_EXIST(cam))
                        {
                            DISPLAY_HUD(false);
                            DISPLAY_RADAR(false);
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);

                            CREATE_CAM(14, out cam);
                            CREATE_CAM(14, out cam2);
                            CREATE_CAM(3, out interpCam);

                            SET_CAM_FOV(cam, 45);
                            SET_CAM_POS(cam, -605.477f, -746.437f, 94.046f);
                            POINT_CAM_AT_PED(cam, Main.PlayerHandle);
                            SET_CAM_ACTIVE(cam, true);
                            SET_CAM_PROPAGATE(cam, true);

                            SET_CAM_FOV(cam2, 60);
                            SET_CAM_POS(cam2, -612.083f, 2116.017f, 170.046f);
                            POINT_CAM_AT_COORD(cam2, -612.083f, -746.286f, 90.046f);

                            SET_CAM_ACTIVE(interpCam, true);
                            SET_CAM_PROPAGATE(interpCam, true);
                            ACTIVATE_SCRIPTED_CAMS(true, true);
                            SET_CAM_INTERP_STYLE_CORE(interpCam, cam, cam2, 1200000, false);
                        }

                        areCreditsRolling = true;
                    }
                }
            }
            if (areCreditsRolling && !ARE_CREDITS_FINISHED())
            {
                if (PedList.Count > 0)
                    PedList.Clear();

                CANCEL_CURRENTLY_PLAYING_AMBIENT_SPEECH(Main.PlayerHandle);

                if (!IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@dance_femidl_b", "loop_b"))
                    _TASK_PLAY_ANIM_NON_INTERRUPTABLE(Main.PlayerHandle, "loop_b", "amb@dance_femidl_b", 4.0f, 1, 0, 0, 0, -1);

                if (NativeControls.IsGameKeyPressed(0, GameKey.Jump) || NativeControls.IsGameKeyPressed(2, GameKey.Jump) ||
                    NativeControls.IsGameKeyPressed(0, GameKey.NavEnter) || NativeControls.IsGameKeyPressed(2, GameKey.NavEnter))
                {
                    CLEAR_ADDITIONAL_TEXT(0, false);
                    REMOVE_ANIMS("amb@dance_femidl_b");
                    CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);

                    SET_CHAR_COORDINATES(Main.PlayerHandle, 1336.286f, -846.782f, 7.843f);
                    SET_CHAR_HEADING(Main.PlayerHandle, 270);

                    if (DOES_CAM_EXIST(cam))
                    {
                        ACTIVATE_SCRIPTED_CAMS(false, false);
                        SET_GAME_CAM_PITCH(0.0f);
                        SET_GAME_CAM_HEADING(0.0f);
                        SET_CAM_BEHIND_PED(Main.PlayerHandle);
                        DESTROY_ALL_CAMS();
                    }
                    DISPLAY_HUD(true);
                    DISPLAY_RADAR(true);
                    SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                    SET_MAX_WANTED_LEVEL(6);
                    CLEAR_WANTED_LEVEL(Main.PlayerIndex);

                    STOP_CREDITS();
                    STOP_END_CREDITS_MUSIC();
                    areCreditsRolling = false;
                }
            }
            else if (areCreditsRolling && ARE_CREDITS_FINISHED())
            {
                CLEAR_ADDITIONAL_TEXT(0, false);
                REMOVE_ANIMS("amb@dance_femidl_b");
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);

                SET_CHAR_COORDINATES(Main.PlayerHandle, 1336.286f, -846.782f, 7.843f);
                SET_CHAR_HEADING(Main.PlayerHandle, 270);

                if (DOES_CAM_EXIST(cam))
                {
                    ACTIVATE_SCRIPTED_CAMS(false, false);
                    SET_GAME_CAM_PITCH(0.0f);
                    SET_GAME_CAM_HEADING(0.0f);
                    SET_CAM_BEHIND_PED(Main.PlayerHandle);
                    DESTROY_ALL_CAMS();
                }
                DISPLAY_HUD(true);
                DISPLAY_RADAR(true);
                SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                SET_MAX_WANTED_LEVEL(6);
                CLEAR_WANTED_LEVEL(Main.PlayerIndex);

                areCreditsRolling = false;
            }
        }
    }
}
