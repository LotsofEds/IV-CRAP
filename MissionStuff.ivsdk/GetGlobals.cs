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
    internal class GetGlobals
    {
        private static bool getVal = false;
        private static SettingsFile varSettings;

        public static void Init()
        {
            varSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\gVars.ini", IVGame.GameStartupPath));
            varSettings.Load();
        }
        private static void getVars(SettingsFile settings, int lowerLimit, int upperLimit)
        {
            if (getVal)
                return;

            // maxLim = 65535
            for (int i = lowerLimit; i < upperLimit; i++)
            {
                if (!settings.DoesKeyExists("MAIN", "var" + i.ToString()))
                    settings.AddKeyToSection("MAIN", "var" + i.ToString());

                //settings.SetValue("MAIN", "var" + i.ToString(), IVTheScripts.GlobalVariables[i].ToString());
                settings.SetValue("MAIN", "var" + i.ToString(), IVTheScripts.GetGlobalInteger(i).ToString());
                settings.Save();
                settings.Load();
            }
            getVal = true;
        }
        public static void Tick()
        {
            if (NativeControls.IsGameKeyPressed(0, GameKey.LookBehind))
            getVars(varSettings, 10000, 15000);
        }
    }
}
