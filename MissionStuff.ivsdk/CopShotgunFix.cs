using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Linq;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class CopShotgunFix
    {
        private static int weaponToGive;
        private static int ammoAmount;
        private static int pWeap = 1;
        public static void Init(SettingsFile settings)
        {
            weaponToGive = settings.GetInteger("COP CAR SHOTGUN FIX", "WeaponToGive", 10);
            ammoAmount = settings.GetInteger("COP CAR SHOTGUN FIX", "AmmoGiven", 8);
        }
        public static void Tick()
        {
            if (IS_CHAR_GETTING_IN_TO_A_CAR(Main.PlayerHandle) && !IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
            {
                GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, 3, out pWeap, out int ammo0, out int ammo2);
                //IVGame.ShowSubtitleMessage(pWeap.ToString());
            }
            else if (pWeap <= 0)
            {
                if (IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
                {
                    GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, 3, out int newWeap, out int ammo0, out int ammo2);
                    //IVGame.ShowSubtitleMessage("ass " + newWeap.ToString());
                    if (newWeap == 11)
                    {
                        REMOVE_WEAPON_FROM_CHAR(Main.PlayerHandle, 11);
                        GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, weaponToGive, ammoAmount, false);
                    }
                    pWeap = 1;
                }
            }
        }
    }
}
