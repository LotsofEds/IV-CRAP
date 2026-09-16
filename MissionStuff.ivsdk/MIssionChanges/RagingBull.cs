using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    // Beat the SONion.
    internal class RagingBull
    {
        // IniShit
        private static string sonModel;
        private static uint sonHealth;

        // OtherShit
        private static int cam1;
        private static int interpCam;
        private static int cam2;
        private static int missionStage;
        private static int uncPed;
        private static int sonPed;
        private static int sonBlip;
        private static int plantObj;
        private static uint fTimer;

        private static uint[] drawArray;
        private static uint[] texArray;
        public static void UnInit()
        {
            fTimer = 0;
            drawArray = null;
            drawArray = null;

            if (DOES_BLIP_EXIST(sonBlip))
                REMOVE_BLIP(sonBlip);
            if (DOES_CHAR_EXIST(sonPed))
                MARK_CHAR_AS_NO_LONGER_NEEDED(sonPed);
            if (DOES_CHAR_EXIST(uncPed))
                MARK_CHAR_AS_NO_LONGER_NEEDED(uncPed);
            if (DOES_OBJECT_EXIST(plantObj))
                MARK_OBJECT_AS_NO_LONGER_NEEDED(plantObj);
        }
        public static void Init(SettingsFile settings)
        {
            sonModel = settings.GetValue("RAGING BULL", "SonModel", "m_y_porient_01");
            sonHealth = settings.GetUInteger("RAGING BULL", "SonHealth", 300);

            drawArray = new uint[3];
            texArray = new uint[3];
            fTimer = 0;
            missionStage = 0;
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("vlad1"))
            {
                // Request the SON model.
                if (!HAS_MODEL_LOADED(GET_HASH_KEY(sonModel)))
                    REQUEST_MODEL(GET_HASH_KEY(sonModel));
                if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_m_porient_01")))
                    REQUEST_MODEL(GET_HASH_KEY("m_m_porient_01"));
                if (!HAVE_ANIMS_LOADED("missbrian_3"))
                    REQUEST_ANIMS("missbrian_3");
                if (!HAVE_ANIMS_LOADED("move_combat_strafe"))
                    REQUEST_ANIMS("move_combat_strafe");

                // Get the unc model variation.
                foreach (var ped in PedHelper.PedHandles)
                {
                    int pedHandle = ped.Value;
                    if (!DOES_CHAR_EXIST(pedHandle))
                        continue;
                    if (!IS_PED_A_MISSION_PED(pedHandle))
                        continue;
                    if (pedHandle == Main.PlayerHandle)
                        continue;
                    if (pedHandle == uncPed)
                        continue;

                    GET_CHAR_MODEL(pedHandle, out uint pModel);
                    if (pModel == GET_HASH_KEY("m_m_porient_01"))
                    {
                        for (int p = 0; p < 3; p++)
                        {
                            drawArray[p] = GET_CHAR_DRAWABLE_VARIATION(pedHandle, p);
                            texArray[p] = GET_CHAR_TEXTURE_VARIATION(pedHandle, (uint)p);
                        }
                    }
                }

                if (missionStage < 12)
                {
                    IVText.TheIVText.ReplaceTextOfTextLabel("V1_COMMAND_07", " ");
                    if (IS_THIS_PRINT_BEING_DISPLAYED("V1_COMMAND_07", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    {
                        CLEAR_PRINTS();
                        CLEAR_BRIEF();
                    }
                }

                // Check if window smashed.
                if (missionStage <= 0 && (IS_NON_FRAG_OBJECT_SMASHED(1280.80f, 399.60f, 22.80f, 1.0f, (unchecked((uint)(-518173288))))
                    || IS_NON_FRAG_OBJECT_SMASHED(1277.80f, 399.60f, 22.80f, 1.0f, 1282124525)))
                {
                    if (!IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                    {
                        IVPadValues[] currPad = IVPad.GetPad().Values;
                        currPad[77].CurrentValue = 255;
                        missionStage++;
                    }
                }
                // Do shit after cutscene.
                else if (missionStage == 1)
                {
                    if (IS_SCREEN_FADED_OUT())
                    {
                        SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                        CREATE_CAM(14, out cam1);

                        SET_CAM_POS(cam1, 1281.491f, 404.074f, 23.997f);
                        SET_CAM_FOV(cam1, 45);
                        POINT_CAM_AT_PED(cam1, Main.PlayerHandle);
                        //POINT_CAM_AT_COORD(cam1, 1284.171f, 400.684f, 22.482f);
                        SET_CAM_ACTIVE(cam1, true);
                        SET_CAM_PROPAGATE(cam1, true);

                        CREATE_OBJECT(GET_HASH_KEY("hospdeadplant01"), 1286.15f, 402.36f, 22.40f, out plantObj, true);
                        SET_OBJECT_DYNAMIC(plantObj, false);
                        ADD_OBJECT_TO_INTERIOR_ROOM_BY_NAME(plantObj, "K_CHINA_Room01");

                        SET_CHAR_COORDINATES(Main.PlayerHandle, new Vector3(1284.171f, 398.943f, 21.482f));
                        SET_CHAR_HEADING(Main.PlayerHandle, 0);

                        CREATE_CHAR((int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY("m_m_porient_01"), 1284.171f, 400.184f, 21.482f, out uncPed, true);
                        SET_CHAR_HEADING(uncPed, 180);
                        SET_ROOM_FOR_CHAR_BY_NAME(uncPed, "K_CHINA_Room01");

                        for (int i = 0; i < 3; i++)
                        {
                            SET_CHAR_COMPONENT_VARIATION(uncPed, (uint)i, drawArray[i], texArray[i]);
                        }
                        missionStage++;
                    }
                }
                // CA_1, CB_2
                // M_M_PORIENT_01
                // Cutscene start.
                else if (missionStage == 2)
                {
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);

                    if (IS_SCREEN_FADING_IN())
                    {
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_r"), 1283, 400, 23, true, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_l"), 1286, 400, 23, true, 0.0f);

                        ACTIVATE_SCRIPTED_CAMS(true, true);
                        CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);

                        DISPLAY_HUD(false);
                        DISPLAY_RADAR(false);
                        GET_GAME_TIMER(out fTimer);
                        missionStage++;
                    }
                }
                // Door opens.
                else if (missionStage == 3)
                {
                    GET_GAME_VIEWPORT_ID(out int viewPort);
                    SET_ROOM_FOR_VIEWPORT_BY_NAME(viewPort, "K_CHINA_Room01");
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);

                    if (!IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "missvlad1", "door_shake"))
                        _TASK_PLAY_ANIM(Main.PlayerHandle, "door_shake", "missvlad1", 4.0f, 0, 0, 1, 0, -1);

                    if (!IS_CHAR_PLAYING_ANIM(uncPed, "missvlad1", "lockandunlock_door"))
                        _TASK_PLAY_ANIM(uncPed, "lockandunlock_door", "missvlad1", 4.0f, 0, 0, 1, 0, -1);

                    if (Main.gTimer >= fTimer + 1000)
                    {
                        CLEAR_CHAR_TASKS(uncPed);
                        _TASK_PLAY_ANIM(uncPed, "walk_strafe_b", "move_combat_strafe", 4.0f, 1, 0, 1, 0, -1);
                        //_TASK_PLAY_ANIM_SECONDARY_NO_INTERRUPT(Main.PlayerHandle, "partial_punch_r", "melee_unarmed_base", 4.0f, 0, 0, 1, 0, -1);

                        GET_GAME_TIMER(out fTimer);
                        missionStage++;
                    }
                }

                // Owner backs off.
                else if (missionStage == 4)
                {
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);

                    if (Main.gTimer >= fTimer + 250)
                    {
                        CLEAR_CHAR_TASKS(Main.PlayerHandle);

                        _TASK_GO_STRAIGHT_TO_COORD(Main.PlayerHandle, 1284.171f, 401.397f, 21.482f, 2, 45000);
                        //_TASK_PLAY_ANIM_SECONDARY(uncPed, "cower_idle", "missbrian_3", 4.0f, 1, 0, 1, 0, -1);
                        //_TASK_PLAY_ANIM_NON_INTERRUPTABLE(uncPed, "shoved_f", "melee_hits_common", 4.0f, 0, 0, 1, 0, -1);

                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_r"), 1283, 400, 23, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_l"), 1286, 400, 23, false, 0.0f);

                        CREATE_CHAR((int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY(sonModel), 1277.365f, 409.829f, 21.482f, out sonPed, true);
                        SET_CHAR_HEADING(sonPed, 215);
                        SET_ROOM_FOR_CHAR_BY_NAME(sonPed, "K_CHINA_Room01");
                        GIVE_WEAPON_TO_CHAR(sonPed, 1, 1, false);
                        SET_CHAR_HEALTH(sonPed, sonHealth);

                        GET_GAME_TIMER(out fTimer);
                        missionStage++;
                    }
                }
                // Niko walks into shop, there should be dialogue here but i think the natives are scuffed.
                else if (missionStage == 5)
                {
                    if (Main.gTimer >= fTimer + 1500)
                    {
                        CLEAR_CHAR_TASKS(uncPed);
                        _TASK_PLAY_ANIM_SECONDARY(uncPed, "cower_idle", "missbrian_3", 4.0f, 1, 0, 1, 0, -1);

                        GET_GAME_TIMER(out fTimer);
                        missionStage++;
                    }
                }
                // Pan to SONion.
                else if (missionStage == 6)
                {
                    if (Main.gTimer >= fTimer + 3000)
                    {
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_r"), 1283, 400, 23, false, 0);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_l"), 1286, 400, 23, false, 0);

                        CREATE_CAM(14, out cam2);
                        CREATE_CAM(3, out interpCam);

                        SET_CAM_POS(cam2, 1281.491f, 404.074f, 23.997f);
                        SET_CAM_FOV(cam2, 45);
                        POINT_CAM_AT_PED(cam2, sonPed);
                        SET_CAM_ACTIVE(interpCam, true);
                        SET_CAM_PROPAGATE(interpCam, true);
                        ACTIVATE_SCRIPTED_CAMS(true, true);
                        SET_CAM_INTERP_STYLE_CORE(interpCam, cam1, cam2, 2000, false);

                        GET_GAME_TIMER(out fTimer);

                        missionStage++;
                    }
                }
                // SON dialogue.
                else if (missionStage == 7)
                {
                    if (Main.gTimer >= fTimer + 1800)
                    {
                        SET_CURRENT_CHAR_WEAPON(sonPed, 1, true);
                        SAY_AMBIENT_SPEECH(sonPed, "Get_out", true, false, 0);
                        missionStage++;
                    }
                }
                // End cutscene.
                else if (missionStage == 8)
                {
                    if (Main.gTimer >= fTimer + 2500)
                    {
                        DO_SCREEN_FADE_OUT(1000);
                        missionStage++;
                    }
                }
                // Clear cutscene stuff.
                else if (missionStage == 9)
                {
                    if (IS_SCREEN_FADED_OUT())
                    {
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_r"), 1283, 400, 23, true, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_l"), 1286, 400, 23, true, 0.0f);

                        DELETE_CHAR(ref uncPed);
                        SET_CHAR_RELATIONSHIP(sonPed, (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);

                        DESTROY_ALL_CAMS();
                        ACTIVATE_SCRIPTED_CAMS(false, false);
                        SET_GAME_CAM_PITCH(0.0f);
                        SET_GAME_CAM_HEADING(0.0f);
                        SET_CAM_BEHIND_PED(Main.PlayerHandle);
                        DISPLAY_HUD(true);
                        DISPLAY_RADAR(true);

                        //SET_CHAR_COORDINATES(Main.PlayerHandle, new Vector3(1284.875f, 401.897f, 21.482f));
                        //SET_CHAR_HEADING(Main.PlayerHandle, 45);

                        SET_CHAR_COORDINATES(Main.PlayerHandle, new Vector3(1285.375f, 402.397f, 21.482f));
                        SET_CHAR_HEADING(Main.PlayerHandle, 270);

                        DO_SCREEN_FADE_IN(1000);
                        missionStage++;
                    }
                }
                // Fight the SON.
                else if (missionStage == 10)
                {
                    if (IS_SCREEN_FADED_IN())
                    {
                        SET_OBJECT_DYNAMIC(plantObj, true);
                        SET_OBJECT_AS_STEALABLE(plantObj, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(plantObj);
                        _TASK_PICKUP_AND_CARRY_OBJECT(Main.PlayerHandle, plantObj, 0, 0, 0, 0);

                        ADD_BLIP_FOR_CHAR(sonPed, out sonBlip);
                        CHANGE_BLIP_COLOUR(sonBlip, (int)eBlipColor.BLIP_COLOR_DARK_RED);
                        CHANGE_BLIP_DISPLAY(sonBlip, (int)eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP);

                        PRINT_NOW("V1_COMMAND_06", 5000, true);
                        SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                        GET_GAME_TIMER(out fTimer);
                        missionStage++;
                    }
                }
                // Object tutorial.
                else if (missionStage == 11)
                {
                    IVText.TheIVText.ReplaceTextOfTextLabel("V1_MELEE_05", "~s~You can throw objects at enemies. Heavier objects will be more effective.");
                    if (Main.gTimer >= fTimer + 2000)
                    {
                        PRINT_HELP("V1_MELEE_05");
                        missionStage++;
                    }
                }
                // After fighting the SON.
                else if (missionStage == 12)
                {
                    IVText.TheIVText.ReplaceTextOfTextLabel("V1_COMMAND_07", "~s~Take the money back to ~y~Vlad.");
                    
                    if (IS_CHAR_INJURED(sonPed))
                    {
                        REMOVE_BLIP(sonBlip);
                        PRINT_NOW("V1_COMMAND_07", 5000, true);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_r"), 1283, 400, 23, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_l"), 1286, 400, 23, false, 0.0f);
                        missionStage++;
                    }
                }
                // Cutscene.
                /*else if (missionStage == 13)
                {
                    IVText.TheIVText.ReplaceTextOfTextLabel("V1_COMMAND_07", "~s~Take the money back to ~y~Vlad.");

                    if (IS_CHAR_INJURED(sonPed))
                    {
                        REMOVE_BLIP(sonBlip);
                        PRINT_NOW("V1_COMMAND_07", 5000, true);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_r"), 1283, 400, 23, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(GET_HASH_KEY_2("cj_new_china_door_l"), 1286, 400, 23, false, 0.0f);
                        missionStage++;
                    }
                }*/
            }
            else if (missionStage > 0)
            {
                UnInit();
                missionStage = 0;
            }
        }
    }
}
