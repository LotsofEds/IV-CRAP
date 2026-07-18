using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class UnrestfulSleep
    {
        private static bool gotStoredHealth;
        private static bool sleepSave;
        private static uint pHealth;
        private static uint pWanted;

        public static void SaveHealthAndWanted(SettingsFile settings, uint health, uint wantedLvl)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "PlayerHealth"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "PlayerHealth");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "WantedLevel"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "WantedLevel");

            settings.SetUInteger(IVGenericGameStorage.ValidSaveName, "PlayerHealth", health);
            settings.SetUInteger(IVGenericGameStorage.ValidSaveName, "WantedLevel", wantedLvl);

            settings.Save();
            settings.Load();
        }
        public static void IngameStart()
        {
            gotStoredHealth = false;
        }
        public static void SaveData()
        {
            if (!sleepSave)
            {
                GET_CHAR_HEALTH(Main.PlayerHandle, out pHealth);
                STORE_WANTED_LEVEL(Main.PlayerIndex, out pWanted);
            }

            if (pHealth > 0 || pWanted > 0)
            {
                if (pHealth <= 0)
                    pHealth = 200;
                SaveHealthAndWanted(Main.savefileSettings, pHealth, pWanted);
            }
        }
        public static void Tick()
        {
            if (!gotStoredHealth)
            {
                pHealth = Main.savefileSettings.GetUInteger(IVGenericGameStorage.ValidSaveName, "PlayerHealth", 0);
                pWanted = Main.savefileSettings.GetUInteger(IVGenericGameStorage.ValidSaveName, "WantedLevel", 0);
                gotStoredHealth = true;
                SetHealthAndWanted();
            }
            if (!IS_SCREEN_FADING() && !IS_SCREEN_FADED_OUT() && (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "lie_on_bed_l") || IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@savegame", "lie_on_bed_r")))
            {
                sleepSave = true;
                GET_CHAR_HEALTH(Main.PlayerHandle, out pHealth);
                STORE_WANTED_LEVEL(Main.PlayerIndex, out pWanted);
            }
            else if (IS_SCREEN_FADING_IN() && (pHealth > 0 || pWanted > 0))
            {
                SetHealthAndWanted();
            }
            else if (IS_PLAYER_CONTROL_ON(Main.PlayerIndex) && (pHealth > 0 || pWanted > 0))
            {
                pHealth = 0;
                pWanted = 0;
            }
        }
        private static void SetHealthAndWanted()
        {
            GET_CHAR_HEALTH(Main.PlayerHandle, out uint currHealth);
            if (currHealth > pHealth && pHealth > 0)
            {
                SET_CHAR_HEALTH(Main.PlayerHandle, pHealth);
                pHealth = 0;
            }

            STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currWanted);
            if (pWanted > currWanted && pWanted > 0)
            {
                ALTER_WANTED_LEVEL(Main.PlayerIndex, pWanted);
                APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                pWanted = 0;
            }
            if (sleepSave)
                sleepSave = false;
        }
    }
}
