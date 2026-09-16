using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;
using static System.Net.Mime.MediaTypeNames;

namespace MissionStuff.ivsdk
{
    internal class MasterBaiter
    {
        // IniShit
        private static bool showIcon;
        private static Vector2 iconPos;
        private static uint timeWindow;
        private static int weaponA;
        private static int weaponB;

        // BooleShit
        private static bool missionStarted;
        private static bool startCutscene;
        private static bool missionEnd;
        private static bool thunderStart;

        // OtherShit
        private static int soundID;
        private static int soundID2;
        private static uint fTimer;
        private static int randInt;
        private static int cam;
        private static int cutsceneSeq;
        private static int tex;

        // GangPeds
        private static int ped1;
        private static int ped2;

        public static void Init(SettingsFile settings)
        {
            showIcon = settings.GetBoolean("MASTER BAITER", "ShowSoundIcon", false);
            iconPos = settings.GetVector2("MASTER BAITER", "IconPos", Vector2.Zero);
            timeWindow = settings.GetUInteger("MASTER BAITER", "SoundMaskTime", 750);
            weaponA = settings.GetInteger("MASTER BAITER", "GangsterOneWeapon", 12);
            weaponB = settings.GetInteger("MASTER BAITER", "GangsterTwoWeapon", 12);
        }
        public static void UnInit()
        {
            RELEASE_SOUND_ID(soundID);
            soundID = 0;
            RELEASE_SOUND_ID(soundID2);
            soundID2 = 0;
            cutsceneSeq = 0;

            ACTIVATE_SCRIPTED_CAMS(false, false);
            DESTROY_ALL_CAMS();

            if (DOES_CHAR_EXIST(ped1) && IS_PED_A_MISSION_PED(ped1))
                MARK_CHAR_AS_NO_LONGER_NEEDED(ped1);
            if (DOES_CHAR_EXIST(ped2) && IS_PED_A_MISSION_PED(ped2))
                MARK_CHAR_AS_NO_LONGER_NEEDED(ped2);

            if (HAS_STREAMED_TXD_LOADED("hud"))
                MARK_STREAMED_TXD_AS_NO_LONGER_NEEDED("hud");
            if (tex > 0)
            {
                RELEASE_TEXTURE(tex);
                tex = 0;
            }

            missionStarted = false;
            startCutscene = false;
            missionEnd = false;
            thunderStart = false;
            fTimer = 0;
        }
        private static void ProcessCutscene()
        {
            if (startCutscene)
            {
                if (cutsceneSeq <= 0 && !missionEnd)
                {
                    CLEAR_PRINTS();
                    DISPLAY_HUD(false);
                    DISPLAY_RADAR(false);

                    CREATE_CHAR((int)ePedType.PED_TYPE_GANG_PUERTO_RICAN, GET_HASH_KEY("m_y_glat_lo_01"), -173.956f, 1377.179f, 32.331f, out ped1, true);
                    CREATE_CHAR((int)ePedType.PED_TYPE_GANG_PUERTO_RICAN, GET_HASH_KEY("m_y_glat_hi_01"), -174.456f, 1376.679f, 32.331f, out ped2, true);

                    GIVE_WEAPON_TO_CHAR(ped1, weaponA, 999, true);
                    GIVE_WEAPON_TO_CHAR(ped2, weaponB, 999, true);

                    SET_CHAR_RELATIONSHIP_GROUP(ped1, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
                    SET_CHAR_RELATIONSHIP_GROUP(ped2, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);

                    SET_CHAR_RELATIONSHIP(ped1, (int)eRelationship.RELATIONSHIP_RESPECT, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
                    SET_CHAR_RELATIONSHIP(ped2, (int)eRelationship.RELATIONSHIP_RESPECT, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);

                    _TASK_FOLLOW_NAV_MESH_TO_COORD(ped1, -175.078f, 1378.081f, 41.634f, 3, -1, 0.1f);
                    _TASK_FOLLOW_NAV_MESH_TO_COORD(ped2, -175.078f, 1378.081f, 41.634f, 3, -1, 0.1f);

                    SET_ROOM_FOR_CHAR_BY_NAME(ped1, "Room_HrlTnGn");
                    SET_ROOM_FOR_CHAR_BY_NAME(ped2, "Room_HrlTnGn");

                    CREATE_CAM(14, out cam);
                    SET_CAM_FOV(cam, 45);
                    SET_CAM_POS(cam, -169.105f, 1376.840f, 32.676f);
                    POINT_CAM_AT_COORD(cam, -172.590f, 1374.672f, 34.198f);

                    SET_CAM_ACTIVE(cam, true);
                    SET_CAM_PROPAGATE(cam, true);
                    ACTIVATE_SCRIPTED_CAMS(true, true);

                    GET_GAME_TIMER(out fTimer);

                    cutsceneSeq = 1;
                }
                else if (cutsceneSeq == 1)
                {
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                    GET_GAME_VIEWPORT_ID(out int viewPort);
                    SET_ROOM_FOR_VIEWPORT_BY_NAME(viewPort, "Room_HrlTnGn");

                    PRINT_HELP_FOREVER("TM_2_30");
                    cutsceneSeq++;
                }
                else if (cutsceneSeq == 2 && Main.gTimer >= fTimer + 5000)
                {
                    SET_CHAR_RELATIONSHIP(ped1, (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                    SET_CHAR_RELATIONSHIP(ped2, (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);

                    SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                    ADD_EXPLOSION(-139.556f, 1376.620f, 33.501f, (int)eExplosion.EXPLOSION_GRENADE, 5, false, true, 0);

                    GET_GAME_TIMER(out fTimer);

                    cutsceneSeq++;
                }
                else if (cutsceneSeq == 3)
                {
                    if (IS_SCREEN_FADED_OUT())
                    {
                        CLEAR_ROOM_FOR_CHAR(ped1);
                        CLEAR_ROOM_FOR_CHAR(ped2);
                        SET_CAM_ACTIVE(cam, false);
                        cutsceneSeq++;
                    }

                    else if (Main.gTimer >= fTimer + 2000)
                    {
                        CLEAR_ROOM_FOR_CHAR(ped1);
                        CLEAR_ROOM_FOR_CHAR(ped2);
                        DISPLAY_HUD(true);
                        DISPLAY_RADAR(true);
                        SET_PLAYER_CONTROL(Main.PlayerIndex, true);

                        ACTIVATE_SCRIPTED_CAMS(false, false);
                        DESTROY_ALL_CAMS();
                        cutsceneSeq++;
                    }
                }
                else if (cutsceneSeq == 4)
                {
                    if (IS_PLAYER_CONTROL_ON(Main.PlayerIndex) && IS_PED_A_MISSION_PED(ped1) && IS_PED_A_MISSION_PED(ped2))
                    {
                        SET_CHAR_COORDINATES(ped1, new Vector3(-177.653f, 1374.586f, 41.135f));
                        SET_CHAR_COORDINATES(ped1, new Vector3(-178.331f, 1374.586f, 41.135f));
                        cutsceneSeq++;
                    }
                }
            }
        }
        private static void ProcessSound()
        {
            if (soundID <= 0)
                soundID = GET_SOUND_ID();
            if (soundID2 <= 0)
                soundID2 = GET_SOUND_ID();

            if (HAS_SOUND_FINISHED(soundID) && Main.gTimer >= fTimer + randInt && thunderStart)
            {
                //IVGame.ShowSubtitleMessage("ass");
                GET_GAME_TIMER(out fTimer);
                randInt = GENERATE_RANDOM_INT_IN_RANGE(8000, 12000);
                if (!IS_INTERIOR_SCENE())
                {
                    float randX = GENERATE_RANDOM_FLOAT_IN_RANGE(-20.0f, 20f);
                    float randY = GENERATE_RANDOM_FLOAT_IN_RANGE(-20.0f, 20f);
                    PLAY_SOUND_FROM_POSITION(soundID, "FLASH_STRIKE_2", Main.PlayerPos.X + randX, Main.PlayerPos.Y + randY, Main.PlayerPos.Z + 20);
                    PLAY_SOUND_FROM_POSITION(soundID2, "THUNDER_CLOSE", Main.PlayerPos.X + randX, Main.PlayerPos.Y + randY, Main.PlayerPos.Z + 20);
                }
                //PLAY_SOUND_FROM_POSITION(soundID, "FLASH_STRIKE_2", Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z + 10);
                //PLAY_SOUND_FROM_POSITION(soundID, "THUNDER_CLOSE", Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z + 10);
            }
            if (IS_INTERIOR_SCENE())
            {
                STOP_SOUND(soundID);
                STOP_SOUND(soundID2);
            }
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("francis4"))
            {
                missionStarted = true;
                GET_CURRENT_WEATHER(out int currWeather);
                if (currWeather != (int)eWeather.WEATHER_RAINING && !IS_SCREEN_FADING_OUT())
                    FORCE_WEATHER_NOW((int)eWeather.WEATHER_RAINING);

                if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_y_glat_lo_01")))
                    REQUEST_MODEL(GET_HASH_KEY("m_y_glat_lo_01"));
                if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_y_glat_hi_01")))
                    REQUEST_MODEL(GET_HASH_KEY("m_y_glat_hi_01"));

                //if (!HAS_STREAMED_TXD_LOADED("hud"))
                    //LOAD_TXD("hud");

                if (showIcon && tex <= 0)
                    tex = GET_TEXTURE_FROM_STREAMED_TXD("hud", "mp_level_mic");

                if (IS_THIS_PRINT_BEING_DISPLAYED("PROMPT_9A", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) && !thunderStart)
                {
                    CLEAR_BRIEF();
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_29", "~s~Lure the ~r~dealer~s~ into the open and take him out. Make sure to mask your gun shot with a loud sound.");
                    PRINT_NOW("TM_2_29", 5000, true);
                    GET_GAME_TIMER(out fTimer);
                    randInt = GENERATE_RANDOM_INT_IN_RANGE(8000, 12000);
                    fTimer -= (timeWindow + 100);
                    thunderStart = true;
                }

                else if (IS_THIS_PRINT_BEING_DISPLAYED("PROMPT_2", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    missionEnd = true;

                if (thunderStart) {
                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!DOES_CHAR_EXIST(pedHandle))
                            continue;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;

                        if (IS_CHAR_PLAYING_ANIM(pedHandle, "missfrancis4", "spooked") || IS_CHAR_PLAYING_ANIM(pedHandle, "missfrancis4", "spooked_2nd_half") || (HAS_CHAR_BEEN_DAMAGED_BY_CHAR(pedHandle, Main.PlayerHandle, true) && !IS_CHAR_INJURED(pedHandle)))
                        {
                            IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "You've spooked the target and now some gang members are investigating your location.");

                            SET_CHAR_ANIM_CURRENT_TIME(pedHandle, "missfrancis4", "spooked", 1.0f);
                            SET_CHAR_ANIM_CURRENT_TIME(pedHandle, "missfrancis4", "spooked_2nd_half", 1.0f);
                            startCutscene = true;
                        }
                    }

                    if (Main.gTimer >= fTimer + timeWindow && IS_CHAR_SHOOTING(Main.PlayerHandle))
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "Some gang members in the apartment heard your shot and aren't too happy about it.");
                        startCutscene = true;
                    }
                    else if (showIcon && Main.gTimer < fTimer + timeWindow && !missionEnd && !startCutscene)
                    {
                        DRAW_SPRITE((uint)tex, ((float)IVGame.Resolution.Height / (float)IVGame.Resolution.Width) * iconPos.X, iconPos.Y, ((float)IVGame.Resolution.Height / (float)IVGame.Resolution.Width) * (0.025f), 0.025f, 0, 255, 255, 255, 255);
                    }
                }

                ProcessCutscene();
                ProcessSound();
            }
            else if (missionStarted)
            {
                UnInit();
            }
        }
    }
}
