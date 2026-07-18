using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Linq;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class KikiAbility
    {
        // IniShit
        private static float kikiStat;
        private static bool keepWeapDeath;

        // WeaponShit
        private static List<int> inventory = new List<int>();
        private static Dictionary<int, int> ammo = new Dictionary<int, int>();

        // OtherShit
        private static bool playerArrested;
        private static bool deathRemove;
        public static void Init(SettingsFile settings)
        {
            kikiStat = settings.GetFloat("BETTER CALL KIKI", "LikeRequirement", 80);
            keepWeapDeath = settings.GetBoolean("BETTER CALL KIKI", "KeepWeaponsOnDeath", false);
        }
        public static void Tick()
        {
            if (GET_FLOAT_STAT(34) > kikiStat)
            {
                if (HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle) && !playerArrested)
                {
                    Main.GetWeaponInventory(true);
                    Main.GetWeaponAmmoCounts();
                    playerArrested = true;
                }
                else if (playerArrested && !HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle) && IS_SCREEN_FADING_IN())
                {
                    RestoreWeapons();
                    playerArrested = false;
                }

                if (keepWeapDeath)
                {
                    if (Main.removeWeapEnable)
                        deathRemove = true;

                    Main.removeWeapEnable = false;
                }
            }
            else
            {
                if (deathRemove)
                    Main.removeWeapEnable = true;
            }
        }
        private static void RestoreWeapons()
        {
            foreach (var weapon in inventory)
            {
                int ammoToGive = ammo.ContainsKey(weapon) ? ammo[weapon] : 0;
                GIVE_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), weapon, ammoToGive, true);
            }
        }
    }
}
