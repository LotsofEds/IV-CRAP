using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class ThirstHunger
    {
        // IniShit
        public static uint thirstTime;
        public static uint hungerTime;
        private static uint healthTime;

        private static uint initThirst;
        private static uint initHunger;

        private static int sleepWaterLoss;
        private static int sleepFoodLoss;

        private static float camShakeAmp;

        private static uint effectThresh;
        private static uint displayTimer;

        // OtherShit
        private static int restoEaten;
        private static int spentDates;
        private static bool getStats;
        private static bool gameStart;
        public static void IngameStart()
        {
            displayTimer = 0;
            gameStart = false;
            getStats = false;
        }
        public static void Init(SettingsFile settings)
        {
            IngameStart();

            thirstTime = settings.GetUInteger("THIRST AND HUNGER", "ThirstDepletionTick", 6000);
            hungerTime = settings.GetUInteger("THIRST AND HUNGER", "HungerDepletionTick", 12000);
            healthTime = settings.GetUInteger("THIRST AND HUNGER", "HealthDepletionTick", 60000);

            initThirst = settings.GetUInteger("THIRST AND HUNGER", "InitWater", 800);
            initHunger = settings.GetUInteger("THIRST AND HUNGER", "InitFood", 500);

            sleepWaterLoss = settings.GetInteger("THIRST AND HUNGER", "SleepDecreaseWater", 80);
            sleepFoodLoss = settings.GetInteger("THIRST AND HUNGER", "SleepDecreaseFood", 40);

            camShakeAmp = settings.GetFloat("THIRST AND HUNGER", "CamShakeAmpMax", 0.1f);

            effectThresh = settings.GetUInteger("THIRST AND HUNGER", "EffectThresh", 100);
        }
        public static void Sleep()
        {
            Main.thirstMeter -= sleepWaterLoss;
            Main.hungerMeter -= sleepFoodLoss;
        }
        public static void SaveThirstAndHunger(SettingsFile settings)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "PlayerWater"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "PlayerWater");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "PlayerFood"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "PlayerFood");

            settings.SetFloat(IVGenericGameStorage.ValidSaveName, "PlayerWater", Main.thirstMeter);
            settings.SetFloat(IVGenericGameStorage.ValidSaveName, "PlayerFood", Main.hungerMeter);

            settings.Save();
            settings.Load();
        }
        public static void Tick()
        {
            if (!gameStart)
            {
                Main.thirstMeter = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "PlayerWater", (int)initThirst);
                Main.hungerMeter = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "PlayerFood", (int)initHunger);

                Main.thirstTimer = 0;
                Main.hungerTimer = 0;
                Main.healthTimer = 0;

                Main.camTimer = 0;
                gameStart = true;
            }
            ProcessDateFood();

            Main.thirstMeter = (int)GenHelp.Clamp(Main.thirstMeter, 0, 1000);
            Main.hungerMeter = (int)GenHelp.Clamp(Main.hungerMeter, 0, 1000);

            if (Main.gTimer >= Main.thirstTimer + thirstTime && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                if (Main.thirstMeter <= effectThresh && !IS_HELP_MESSAGE_BEING_DISPLAYED() && Main.gTimer >= displayTimer + 60000)
                {
                    GET_GAME_TIMER(out displayTimer);
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_5_6", "You are thirsty, grab something to drink.");
                    PRINT_HELP("TM_5_6");
                }

                if (Main.thirstMeter > 0)
                    Main.thirstMeter--;
                GET_GAME_TIMER(out Main.thirstTimer);
            }
            if (Main.gTimer >= Main.hungerTimer + hungerTime && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                if (Main.hungerMeter <= effectThresh && !IS_HELP_MESSAGE_BEING_DISPLAYED() && Main.gTimer >= displayTimer + 60000)
                {
                    GET_GAME_TIMER(out displayTimer);
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_5_6", "You are hungry, grab something to eat.");
                    PRINT_HELP("TM_5_6");
                }

                if (Main.hungerMeter > 0)
                    Main.hungerMeter--;
                GET_GAME_TIMER(out Main.hungerTimer);
            }
            if (Main.thirstMeter < effectThresh || Main.hungerMeter < effectThresh)
            {
                // Compare highest cam shake value between thirst, hunger, and sleep.
                if (Main.camShake < camShakeAmp * ((float)(int)(effectThresh - Main.thirstMeter) / effectThresh))
                    Main.camShake = camShakeAmp * ((float)(int)(effectThresh - Main.thirstMeter) / effectThresh);
                if (Main.camShake < camShakeAmp * ((float)(int)(effectThresh - Main.hungerMeter) / effectThresh))
                    Main.camShake = camShakeAmp * ((float)(int)(effectThresh - Main.hungerMeter) / effectThresh);

                if (!IS_CAM_SHAKING() && IS_PLAYER_CONTROL_ON(Main.PlayerIndex) && !Main.PlayerPed.PedFlags4.IsDrunk && Main.gTimer >= Main.camTimer + 5000)
                {
                    SET_DRUNK_CAM(NativeCamera.GetGameCam().Handle, Main.camShake, 5000);
                    GET_GAME_TIMER(out Main.camTimer);
                }
                if ((Main.thirstMeter == 0 || Main.hungerMeter == 0) && Main.gTimer >= Main.healthTimer + healthTime)
                {
                    GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);
                    SET_CHAR_HEALTH(Main.PlayerHandle, (pHealth - 1));
                    GET_GAME_TIMER(out Main.healthTimer);
                }
            }
            else
                GET_GAME_TIMER(out Main.healthTimer);

            if (IS_CHAR_DEAD(Main.PlayerHandle))
            {
                Main.thirstMeter = 500;
                Main.hungerMeter = 300;

                GET_GAME_TIMER(out Main.thirstTimer);
                GET_GAME_TIMER(out Main.hungerTimer);
            }
        }
        private static void ProcessDateFood()
        {
            if (!getStats)
            {
                restoEaten = GET_INT_STAT(306);
                spentDates = GET_INT_STAT(91);
                getStats = true;
            }
            else if (getStats)
            {
                if (restoEaten < GET_INT_STAT(306) && spentDates < GET_INT_STAT(91))
                {
                    Main.thirstMeter = 1000;
                    Main.hungerMeter = 1000;

                    GET_GAME_TIMER(out Main.thirstTimer);
                    GET_GAME_TIMER(out Main.hungerTimer);
                    getStats = false;
                }
            }
        }
    }
}
