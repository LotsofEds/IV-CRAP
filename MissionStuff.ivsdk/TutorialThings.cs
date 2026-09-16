using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class TutorialThings
    {
        private static bool tutDisplay;

        // CutsceneShit
        private static int prop1;
        private static int actor1;
        private static int actor2;
        private static int cam1;
        private static int cam2;
        private static int interpCam;
        private static int cutsceneSeq;

        private static uint fTimer;

        public static void Init()
        {
            cutsceneSeq = -1;
        }

        public static void UnInit()
        {
            cutsceneSeq = -1;

            REMOVE_ANIMS("amb@sprunk_plyr");
            REMOVE_ANIMS("missvlad1");
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("m_m_genbum_01"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("m_m_crackhead"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("cspillbottle"));

            if (DOES_CHAR_EXIST(actor1))
                MARK_CHAR_AS_NO_LONGER_NEEDED(actor1);
            if (DOES_CHAR_EXIST(actor2))
                MARK_CHAR_AS_NO_LONGER_NEEDED(actor2);
            if (DOES_OBJECT_EXIST(prop1))
                MARK_OBJECT_AS_NO_LONGER_NEEDED(prop1);

            DESTROY_ALL_CAMS();
            ACTIVATE_SCRIPTED_CAMS(false, false);
            DISPLAY_HUD(true);
            DISPLAY_RADAR(true);
            SET_PLAYER_CONTROL(Main.PlayerIndex, true);
        }
        public static void IngameStart()
        {
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("roman2"))
            {
                if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_m_genbum_01")))
                    REQUEST_MODEL(GET_HASH_KEY("m_m_genbum_01"));
                if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_m_crackhead")))
                    REQUEST_MODEL(GET_HASH_KEY("m_m_crackhead"));
                if (!HAS_MODEL_LOADED(GET_HASH_KEY("cspillbottle")))
                    REQUEST_MODEL(GET_HASH_KEY("cspillbottle"));

                IVText.TheIVText.ReplaceTextOfTextLabel("R2_MISC_2", "~s~Use ~PAD_RSTICK_ALL~ to look around.");

                // If last tutorial text is shown, init cutscene (create cams and actors)..
                if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("R2_ANSWERTUT_4") && !DOES_CAM_EXIST(cam1))
                {
                    if (!HAVE_ANIMS_LOADED("missvlad1"))
                        REQUEST_ANIMS("missvlad1");
                    if (!HAVE_ANIMS_LOADED("amb@sprunk_plyr"))
                        REQUEST_ANIMS("amb@sprunk_plyr");

                    CREATE_CHAR((int)ePedType.PED_TYPE_BUM, GET_HASH_KEY("m_m_genbum_01"), 1218.128f, 66.996f, 36.804f, out actor1, true);
                    CREATE_CHAR((int)ePedType.PED_TYPE_BUM, GET_HASH_KEY("m_m_crackhead"), 1225.132f, 66.325f, 36.804f, out actor2, true);

                    SET_CHAR_HEADING(actor1, 285);
                    SET_CHAR_HEADING(actor2, 115);

                    _TASK_GO_STRAIGHT_TO_COORD(actor1, 1221.028f, 67.096f, 36.804f, 2, 45000);
                    _TASK_GO_STRAIGHT_TO_COORD(actor2, 1222.632f, 67.325f, 36.804f, 2, 45000);

                    CREATE_OBJECT(GET_HASH_KEY("cspillbottle"), Vector3.Zero, out prop1, true);

                    DISPLAY_HUD(false);
                    DISPLAY_RADAR(false);
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                    CREATE_CAM(14, out cam1);
                    SET_CAM_POS(cam1, 1221.841f, 79.798f, 39.713f);
                    SET_CAM_FOV(cam1, 45);
                    POINT_CAM_AT_COORD(cam1, 1222.341f, 66.798f, 38.213f);
                    SET_CAM_ACTIVE(cam1, true);
                    SET_CAM_PROPAGATE(cam1, true);
                    GET_GAME_TIMER(out fTimer);

                    cutsceneSeq = 0;
                }
                else if (DOES_CAM_EXIST(cam1))
                {
                    if (cutsceneSeq >= 0)
                    {
                        DISPLAY_HUD(false);
                        DISPLAY_RADAR(false);
                        SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                    }
                    // Interp from game cam to scripted cam.
                    if (Main.gTimer >= fTimer + 2000 && cutsceneSeq == 0)
                    {
                        ATTACH_OBJECT_TO_PED(prop1, actor1, (uint)eBone.BONE_RIGHT_HAND, 0.1f, 0.02f, -0.02f, 0f, 0f, 0f, 0);
                        SET_INTERP_FROM_GAME_TO_SCRIPT(true, 5000);
                        ACTIVATE_SCRIPTED_CAMS(true, true);

                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_27", "Pills can be bought around various locations around the city.");
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_29", "Different pills will have different effects on the player. Taking pills too often will reduce their effectiveness.");
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_26", "To take pills, hold ~PAD_BACK~ to bring up the interaction menu and select ~y~Pills.");

                        CREATE_CAM(14, out cam2);
                        CREATE_CAM(3, out interpCam);
                        cutsceneSeq++;
                    }

                    // Make peds pass pill obj.
                    if (Main.gTimer >= fTimer + 7000 && cutsceneSeq == 1)
                    {
                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_27"))
                            PRINT_HELP("TM_2_27");

                        SET_CHAR_COORDINATES(actor1, 1221.528f, 67.096f, 36.804f);
                        SET_CHAR_COORDINATES(actor2, 1222.632f, 67.325f, 36.804f);
                        SET_CHAR_HEADING(actor1, 285);
                        SET_CHAR_HEADING(actor2, 115);

                        _TASK_PLAY_ANIM_NON_INTERRUPTABLE(actor1, "give_obj", "missvlad1", 4.0f, 0, 0, 1, 0, -1);
                        _TASK_PLAY_ANIM_NON_INTERRUPTABLE(actor2, "take_obj", "missvlad1", 4.0f, 0, 0, 1, 0, -1);
                        REMOVE_ANIMS("missvlad1");

                        SET_CAM_POS(cam1, 1223.906f, 63.588f, 38.522f);
                        SET_CAM_FOV(cam1, 60);
                        cutsceneSeq++;
                    }
                    // Zoomed out cam.
                    else if (cutsceneSeq == 2)
                    {
                        GET_CHAR_ANIM_CURRENT_TIME(actor1, "missvlad1", "give_obj", out float animTime);
                        if (animTime > 0.6f)
                        {
                            DETACH_OBJECT(prop1, true);
                            ATTACH_OBJECT_TO_PED(prop1, actor2, (uint)eBone.BONE_RIGHT_HAND, 0.1f, 0.02f, -0.02f, 0f, 0f, 0f, 0);
                        }

                        if (Main.gTimer >= fTimer + 9500)
                        {
                            SET_CAM_POS(cam1, 1227.183f, 16.127f, 38.065f);
                            SET_CAM_FOV(cam1, 30);
                            cutsceneSeq++;
                        }
                    }
                    // Pan cam to pharmacy.
                    else if (Main.gTimer >= fTimer + 10000 && cutsceneSeq == 3)
                    {
                        SET_CAM_POS(cam2, 1227.183f, 16.127f, 38.065f);
                        SET_CAM_FOV(cam2, 60);
                        POINT_CAM_AT_COORD(cam2, 1236.124f, 23.133f, 38.813f);
                        SET_CAM_ACTIVE(interpCam, true);
                        SET_CAM_PROPAGATE(interpCam, true);
                        ACTIVATE_SCRIPTED_CAMS(true, true);
                        SET_CAM_INTERP_STYLE_CORE(interpCam, cam1, cam2, 2000, false);
                        cutsceneSeq++;
                    }
                    // Print second help message.
                    else if (Main.gTimer >= fTimer + 12500 && cutsceneSeq == 4)
                    {
                        MARK_CHAR_AS_NO_LONGER_NEEDED(actor1);

                        PRINT_HELP("TM_2_29");
                        SET_CAM_POS(cam1, 1223.906f, 63.588f, 38.522f);
                        SET_CAM_FOV(cam1, 60);
                        cutsceneSeq++;
                    }
                    // Switch cam back to ped taking pill.
                    else if (cutsceneSeq == 5)
                    {
                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_29"))
                            PRINT_HELP_WITH_STRING_NO_SOUND("TM_2_29", "");

                        if (Main.gTimer >= fTimer + 17500)
                        {
                            _TASK_PLAY_ANIM_SECONDARY(actor2, "partial_drink", "amb@sprunk_plyr", 4.0f, 0, 0, 1, 0, -1);
                            REMOVE_ANIMS("amb@sprunk_plyr");

                            PRINT_HELP("TM_2_26");

                            SET_CAM_ACTIVE(cam1, true);
                            SET_CAM_ACTIVE(interpCam, false);
                            SET_CAM_ACTIVE(cam2, false);

                            cutsceneSeq++;
                        }
                    }
                    // End cutscene.
                    else if (cutsceneSeq == 6)
                    {
                        if (Main.gTimer >= fTimer + 21000)
                        {
                            DELETE_OBJECT(ref prop1);

                            MARK_CHAR_AS_NO_LONGER_NEEDED(actor2);
                            DESTROY_ALL_CAMS();
                            ACTIVATE_SCRIPTED_CAMS(false, false);
                            SET_GAME_CAM_PITCH(0.0f);
                            SET_GAME_CAM_HEADING(0.0f);
                            SET_CAM_BEHIND_PED(Main.PlayerHandle);
                            DISPLAY_HUD(true);
                            DISPLAY_RADAR(true);
                            SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                            cutsceneSeq = -1;
                        }
                    }
                }

                //ProcessShowNextHelpMsg("R2_ANSWERTUT_4", "TM_2_27", "Pills can be bought around various locations around the city.");
                //ProcessShowNextHelpMsg("TM_2_27", "TM_2_29", "Different pills will have different effects on the player. Taking pills too often will reduce their effectiveness.");
                //ProcessShowNextHelpMsg("TM_2_29", "TM_2_26", "To take pills, hold ~PAD_BACK~ to bring up the interaction menu and select ~y~Pills.");
            }
            else if (cutsceneSeq >= 0)
                UnInit();

            else if (NativeGame.IsScriptRunning("faustin4"))
                IVText.TheIVText.ReplaceTextOfTextLabel("FAU4_TUT_01", "~s~When aiming, use ~PAD_LSTICK_LEFTRIGHT~ and press ~INPUT_JUMP~ to roll.");

            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("ROBA_1") || IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("ROBB_1") || IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("ROBC_1"))
            {
                if (NativeControls.IsGameKeyPressed(2, GameKey.NavEnter) || NativeControls.IsGameKeyPressed(2, GameKey.EnterCar))
                    CLEAR_HELP();
            }
        }
        /*private static void ProcessShowNextHelpMsg(string gxtTrigger, string nextTextToShow, string newString)
        {
            IVText.TheIVText.ReplaceTextOfTextLabel(nextTextToShow, newString);
            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED(gxtTrigger))
                tutDisplay = true;
            if (tutDisplay && !IS_HELP_MESSAGE_BEING_DISPLAYED())
            {
                PRINT_HELP(nextTextToShow);
                tutDisplay = false;
            }
        }*/
    }
}
