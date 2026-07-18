using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class GiveYouSharesNB
    {
        // IniShit
        private static bool debug;
        private static float romanStat;
        private static int revenueGiven;
        private static int maxRevenue;

        // OtherShit
        private static bool atmOn;
        private static bool getTimer;
        private static bool getDay;
        private static uint fTimer;
        private static int daysPassed;
        private static int currentBal;
        public static void Init(SettingsFile settings)
        {
            debug = settings.GetBoolean("I'LL GIVE YOU SHARES, NB", "Debug", false);
            romanStat = settings.GetFloat("I'LL GIVE YOU SHARES, NB", "LikeRequirement", 80);
            revenueGiven = settings.GetInteger("I'LL GIVE YOU SHARES, NB", "RevenueGiven", 200);
            maxRevenue = settings.GetInteger("I'LL GIVE YOU SHARES, NB", "MaxRevenue", 1000);
        }
        public static void IngameStart()
        {
            getDay = false;
        }
        public static void Tick()
        {
            if (!getDay)
            {
                if (!Main.savefileSettings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "CurrentBalance"))
                    Main.savefileSettings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "CurrentBalance");

                currentBal = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "CurrentBalance", 0);
                daysPassed = GET_INT_STAT(260);
                getDay = true;
            }
            else if (daysPassed != GET_INT_STAT(260))
            {
                daysPassed = GET_INT_STAT(260);

                if (GET_FLOAT_STAT(1) > romanStat)
                {
                    if (currentBal < maxRevenue)
                        currentBal += revenueGiven;
                    if (currentBal > maxRevenue)
                        currentBal = maxRevenue;
                }
            }
            if (NativeGame.IsScriptRunning("atmobj") && GET_FLOAT_STAT(10) > 40)
            {
                if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("ATM_05"))
                {
                    getTimer = false;
                    atmOn = true;
                }
                else if (atmOn && IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("ObjScpt_03"))
                {
                    getTimer = false;
                    if (debug)
                        IVGame.ShowSubtitleMessage("ATM On");
                    if (currentBal > 0)
                    {
                        ADD_SCORE(Main.PlayerIndex, currentBal);
                        currentBal = 0;
                    }
                }
                else if (atmOn)
                {
                    if (!getTimer)
                    {
                        GET_GAME_TIMER(out fTimer);
                        getTimer = true;
                    }
                    else if (getTimer && (Main.gTimer >= fTimer + 1000))
                    {
                        if (debug)
                            IVGame.ShowSubtitleMessage("ATM Off");
                        getTimer = false;
                        atmOn = false;
                    }
                }
            }
        }
        public static void SaveMoney(SettingsFile settings)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "CurrentBalance"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "CurrentBalance");

            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "CurrentBalance", currentBal);

            settings.Save();
            settings.Load();
        }
    }
}
