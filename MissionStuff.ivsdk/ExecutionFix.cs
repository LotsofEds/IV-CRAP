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
    internal class ExecutionFix
    {
        private static int playerWeapon;
        private static int playerAmmo;
        private static bool fireShot;
        private static bool isReadyToExecute;
        private static uint fTimer;

        public static readonly List<int> WeaponList = new List<int>();

        public static void Init(SettingsFile settings)
        {
            string WeapString = settings.GetValue("EXECUTION COMPATIBILITY", "WeaponExceptions", "-1");

            WeaponList.Clear();
            foreach (var WeaponValue in WeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                WeaponList.Add(WeaponID);
            }
        }
        public static void Tick()
        {
            GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, 2, out int pWeap, out int pAmmo1, out int pAmmo2);
            GET_CURRENT_CHAR_WEAPON(Main.PlayerHandle, out int currWeap);

            if (pWeap > 0 && currWeap == pWeap)
            {
                bool cantExecute = false;
                foreach (int weap in WeaponList)
                {
                    if (currWeap == weap)
                    {
                        cantExecute = true;
                        break;
                    }
                }
                if (!cantExecute)
                {
                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!DOES_CHAR_EXIST(pedHandle))
                            continue;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;

                        if (!isReadyToExecute && GET_CHAR_READY_TO_BE_EXECUTED(pedHandle) && IS_PLAYER_FREE_AIMING_AT_CHAR(Main.PlayerIndex, pedHandle) && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                        {
                            if (IS_CONTROL_JUST_PRESSED(0, (int)eGameKey.GAME_KEY_ATTACK) || IS_CONTROL_JUST_PRESSED(2, (int)eGameKey.GAME_KEY_ATTACK))
                            {
                                GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, 2, out playerWeapon, out playerAmmo, out int pAmmo3);
                                GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, 7, 20, true);
                                isReadyToExecute = true;
                                GET_GAME_TIMER(out fTimer);
                            }
                        }
                        else if (isReadyToExecute)
                        {
                            if (!IS_PLAYER_CONTROL_ON(Main.PlayerIndex) || fTimer + 50 < Main.gTimer)
                            {
                                REMOVE_WEAPON_FROM_CHAR(Main.PlayerHandle, 7);
                                GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, playerWeapon, playerAmmo, true);
                                if (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "missray3", "ex_niko_shoot"))
                                    fireShot = true;

                                isReadyToExecute = false;
                            }
                        }
                    }
                    if (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "missray3", "ex_niko_shoot") && fireShot)
                    {
                        GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "missray3", "ex_niko_shoot", out float animTime);
                        if (!IS_CHAR_SHOOTING(Main.PlayerHandle) && currWeap == playerWeapon)
                        {
                            GET_OFFSET_FROM_CHAR_IN_WORLD_COORDS(Main.PlayerHandle, 0, 2, 0, out float offX, out float offY, out float offZ);
                            FIRE_PED_WEAPON(Main.PlayerHandle, new Vector3(offX, offY, offZ));
                            fireShot = false;
                        }
                    }
                    else
                        fireShot = false;
                }
            }
        }
    }
}
