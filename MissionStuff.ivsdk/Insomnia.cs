using CCL.GTAIV;
using IVSDKDotNet;
using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class Insomnia
    {
        // IniShit
        public static uint sleepTime;
        private static uint initSleep;
        private static uint sleepMin;

        private static float camShakeAmp;

        private static uint dimAmt;
        private static float movePenalty;

        // OtherShit
        private static bool gameStart;
        private static bool isSleeping;
        private static bool hideScreen;
        private static uint skipTimer;
        private static uint displayTimer;
        public static void IngameStart()
        {
            gameStart = false;
        }
        public static void Init(SettingsFile settings)
        {
            IngameStart();

            sleepTime = settings.GetUInteger("GO GET SOME SLEEP", "SleepDepletionTick", 6000);
            initSleep = settings.GetUInteger("GO GET SOME SLEEP", "InitTiredness", 200);
            sleepMin = settings.GetUInteger("GO GET SOME SLEEP", "EffectThresh", 100);

            camShakeAmp = settings.GetFloat("GO GET SOME SLEEP", "CamShakeAmpMax", 0.1f);

            dimAmt = settings.GetUInteger("GO GET SOME SLEEP", "ScreenDimAmountMax", 50);
            movePenalty = settings.GetFloat("GO GET SOME SLEEP", "MaxMovePenalty", 0.85f);
        }
        public static void SaveSleepAmount(SettingsFile settings)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "PlayerSleep"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "PlayerSleep");

            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "PlayerSleep", Main.sleepMeter);

            settings.Save();
            settings.Load();
        }
        public static void Tick()
        {
            if (!gameStart)
            {
                Main.sleepMeter = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "PlayerSleep", (int)initSleep);

                Main.camTimer = 0;
                Main.sleepTimer = 0;
                gameStart = true;
            }

            Main.sleepMeter = (int)GenHelp.Clamp(Main.sleepMeter, 0, 1000);

            if (!IS_SCREEN_FADING_OUT() && (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "lie_on_bed_l") || IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "lie_on_bed_r")))
            {
                Main.sleepMeter = 1000;
                isSleeping = true;

                GET_GAME_TIMER(out Main.sleepTimer);
            }
            if (isSleeping)
            {
                if (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "lie_on_bed_l") || IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "lie_on_bed_r")
                    || IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "get_out_bed_l") || IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "get_out_bed_r"))
                {
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_5_8", "Hold ~INPUT_SPRINT~ to skip the animation.");
                    if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_5_8"))
                    PRINT_HELP_FOREVER("TM_5_8");

                    if (!NativeControls.IsGameKeyPressed(2, GameKey.Sprint))
                        GET_GAME_TIMER(out skipTimer);
                    else
                    {
                        if (Main.gTimer >= skipTimer + 500)
                        {
                            SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@savegame", "lie_on_bed_l", 1.0f);
                            SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@savegame", "lie_on_bed_r", 1.0f);
                            SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@savegame", "get_out_bed_l", 1.0f);
                            SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@savegame", "get_out_bed_r", 1.0f);
                            hideScreen = true;
                        }
                    }
                    if (IS_SCREEN_FADING_IN())
                        hideScreen = false;
                    else if (!IS_SCREEN_FADED_OUT() && hideScreen)
                        DRAW_RECT(0.5f, 0.5f, 1, 1, 0, 0, 0, 255);
                }
                else
                {
                    CLEAR_HELP();

                    hideScreen = false;
                    isSleeping = false;
                }
            }
            if (Main.gTimer >= Main.sleepTimer + sleepTime && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                if (Main.sleepMeter <= sleepMin && !IS_HELP_MESSAGE_BEING_DISPLAYED() && Main.gTimer >= displayTimer + 60000)
                {
                    GET_GAME_TIMER(out displayTimer);
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_5_8", "You are sleepy, go get some sleep.");
                    PRINT_HELP("TM_5_8");
                }

                if (Main.sleepMeter > 0)
                Main.sleepMeter--;
                GET_GAME_TIMER(out Main.sleepTimer);
            }
            if (Main.sleepMeter <= sleepMin)
            {
                float moveSpd = (1.0f - movePenalty) * ((float)((int)sleepMin - Main.sleepMeter) / sleepMin);
                if (IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, 1.0f - moveSpd);

                // Compare highest cam shake value between thirst, hunger, and sleep.
                if (Main.camShake < camShakeAmp * ((float)(int)(sleepMin - Main.sleepMeter) / sleepMin))
                    Main.camShake = camShakeAmp * ((float)(int)(sleepMin - Main.sleepMeter) / sleepMin);

                if (!IS_CAM_SHAKING() && IS_PLAYER_CONTROL_ON(Main.PlayerIndex) && !Main.PlayerPed.PedFlags4.IsDrunk && Main.gTimer >= Main.camTimer + 5000)
                {
                    SET_DRUNK_CAM(NativeCamera.GetGameCam().Handle, Main.camShake, 5000);
                    GET_GAME_TIMER(out Main.camTimer);
                }
                float rectOpacity = dimAmt * ((float)(int)(sleepMin - Main.sleepMeter) / sleepMin);
                DRAW_RECT(0.5f, 0.5f, 1, 1, 0, 0, 0, (int)rectOpacity);
            }

            if (IS_CHAR_DEAD(Main.PlayerHandle))
            {
                Main.sleepMeter = 800;

                GET_GAME_TIMER(out Main.sleepTimer);
            }
        }
    }
}
