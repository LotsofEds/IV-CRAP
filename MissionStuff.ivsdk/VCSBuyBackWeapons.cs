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
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class VCSBuyBackWeapons
    {
        // IniShit
        private static bool buyBackWeapons;
        private static int bribeDuration;

        // WeaponShit
        private static List<int> inventory = new List<int>();
        private static Dictionary<int, int> ammo = new Dictionary<int, int>();
        private static int fullPrice;

        // OtherShit
        private static bool playerArrested;
        private static bool deathArrest;
        private static bool canBribe;
        private static int bribeBlip;
        private static Vector3 bribeLoc;
        private static uint fTimer;
        public static void Init(SettingsFile settings)
        {
            buyBackWeapons = settings.GetBoolean("REMOVE WEAPONS ON DEATH", "BuyBackWeapons", false);
            bribeDuration = settings.GetInteger("REMOVE WEAPONS ON DEATH", "BribeDuration", 360000);
        }
        public static void UnInit()
        {
            inventory.Clear();
            ammo.Clear();
            deathArrest = false;
            playerArrested = false;
            REMOVE_BLIP(bribeBlip);
        }
        public static void IngameStart()
        {
            fTimer = 0;
        }
        public static void Tick()
        {
            if ((IS_CHAR_DEAD(Main.PlayerHandle) || HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle)) && !deathArrest)
            {
                inventory = Main.GetWeaponInventory(true);
                ammo = Main.GetWeaponAmmoCounts();

                if (IS_CHAR_DEAD(Main.PlayerHandle))
                {
                    REMOVE_ALL_CHAR_WEAPONS(Main.PlayerPed.GetHandle());
                    SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_UNARMED, true);
                }
                else
                    playerArrested = true;

                if (buyBackWeapons)
                    deathArrest = true;
            }
            else if (deathArrest && IS_SCREEN_FADING_IN() && !IS_CHAR_DEAD(Main.PlayerHandle) && !HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle))
            {
                canBribe = false;
                bribeLoc = Main.PlayerPos;
                ShowBribeLocation();
                if (!DOES_BLIP_EXIST(bribeBlip) && canBribe)
                {
                    // 1371.646, 621.487, 35.829
                    ADD_BLIP_FOR_COORD(bribeLoc.X, bribeLoc.Y, bribeLoc.Z, out bribeBlip);

                    NativeBlip pBlip = new NativeBlip(bribeBlip);

                    if (!playerArrested)
                        pBlip.Icon = BlipIcon.Building_Hospital;
                    else
                        pBlip.Icon = BlipIcon.Building_PoliceStation;
                    pBlip.Name = "Weapon Bribe";
                    pBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;
                    pBlip.ShowOnlyWhenNear = true;
                }
                GET_GAME_TIMER(out fTimer);
                //IVGame.ShowSubtitleMessage(Main.PlayerPos.ToString());
                deathArrest = false;
            }
            if (canBribe && IS_SCREEN_FADED_IN())
            {
                if (LOCATE_CHAR_ON_FOOT_3D(Main.PlayerHandle, bribeLoc.X, bribeLoc.Y, bribeLoc.Z, 1.0f, 1.0f, 1.0f, true))
                {
                    ShowBribeLocation();
                    if (NativeControls.IsGameKeyPressed(0, GameKey.Action))
                        RestoreWeapons();
                }
                else if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_28"))
                    CLEAR_HELP();
                if (Main.gTimer > fTimer + bribeDuration)
                {
                    REMOVE_BLIP(bribeBlip);
                    canBribe = false;
                }
            }
        }

        private static void ShowBribeLocation()
        {
            if (!canBribe)
            {
                BribeForWeaponsNotification(inventory);
                if (fullPrice > 0)
                    canBribe = true;
            }
            else if (!IS_HELP_MESSAGE_BEING_DISPLAYED())
                PRINT_HELP_FOREVER("TM_2_28");
        }

        private static void RestoreWeapons()
        {
            if (Main.PlayerPed.PlayerInfo.GetMoney() < fullPrice)
                return;

            PLAY_SOUND_FRONTEND(-1, "FRONTEND_OTHER_INFO");

            foreach (var weapon in inventory)
            {
                int ammoToGive = ammo.ContainsKey(weapon) ? ammo[weapon] : 0;
                GIVE_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), weapon, ammoToGive, true);
            }

            IVPlayerInfoExtensions.RemoveMoney(Main.PlayerPed.PlayerInfo, fullPrice);
            fullPrice = 0;

            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_28"))
                CLEAR_HELP();

            REMOVE_BLIP(bribeBlip);

            canBribe = false;
        }
        private static int GetWeaponBribePrice(int weapon)
        {
            int weapPrice = Main.bribeSettings.GetInteger(weapon.ToString(), "WeaponBribePrice", 500);
            return weapPrice;
        }
        private static int GeAmmoBribePrice(int weapon)
        {
            int ammoPrice = Main.bribeSettings.GetInteger(weapon.ToString(), "AmmoBribePriceMult", 2);
            return ammoPrice;
        }
        private static void BribeForWeaponsNotification(List<int> inventory)
        {
            CalculateBribePrice(inventory);
            string message;

            if (fullPrice == 0) return;

            if (Main.PlayerPed.PlayerInfo.GetMoney() < fullPrice)
            {
                message = "You don't have enough money to bribe.";
            }
            else
            {
                message = "Pay ~g~$" + fullPrice.ToString() + "~s~ bribe to get your weapons back? ~n~Press ~INPUT_PICKUP~ to pay.";
            }

            IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_28", message);
        }
        private static void CalculateBribePrice(List<int> inventory)
        {
            fullPrice = 0;

            foreach (int weapon in inventory)
            {
                fullPrice += GetWeaponBribePrice(weapon);
                fullPrice += ammo.ContainsKey(weapon) ? (ammo[weapon] * GeAmmoBribePrice(weapon)) : 0;
            }
        }
    }
}
