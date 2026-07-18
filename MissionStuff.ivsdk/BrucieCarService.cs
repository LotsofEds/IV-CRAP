using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Linq;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class BrucieCarService
    {
        // IniShit
        private static List<string> carList = new List<string>();
        private static List<int> weapList = new List<int>();
        private static int baseMoneyMin;
        private static int baseMoneyMax;
        private static int extraMoneyAmt;
        private static int armorAmt;

        // BooleShit
        private static bool spawnTheCar;

        // ListShit
        private static List<int> giveWeaponList = new List<int>();
        private static List<LocationData> locationList = new List<LocationData>();
        //private static List<float> spawnLocList = new List<float>();

        // OtherShit
        private static int carBlip;
        private static int brucieCar;
        private static uint fTimer;
        private static uint timeLimit;

        // SettingsFile
        private static SettingsFile brucieSettings;

        public static void Init(SettingsFile settings)
        {
            string carString = settings.GetValue("NOW *THIS* IS HOW WE ROLL", "CarList", "");
            foreach (var carModel in carString.Split(','))
                carList.Add(carModel);

            string weapString = settings.GetValue("NOW *THIS* IS HOW WE ROLL", "WeaponList", "0");
            foreach (var weaponVal in weapString.Split(','))
            {
                int weapon = Int32.Parse(weaponVal.Trim());
                weapList.Add(weapon);
            }

            baseMoneyMin = settings.GetInteger("NOW *THIS* IS HOW WE ROLL", "BaseMoneyAmountMin", 0);
            baseMoneyMax = settings.GetInteger("NOW *THIS* IS HOW WE ROLL", "BaseMoneyAmountMax", 0);
            extraMoneyAmt = settings.GetInteger("NOW *THIS* IS HOW WE ROLL", "ExtraMoneyMaxAmount", 0);
            armorAmt = settings.GetInteger("NOW *THIS* IS HOW WE ROLL", "ArmorAmount", 0);
            timeLimit = settings.GetUInteger("NOW *THIS* IS HOW WE ROLL", "DespawnTime", 120000);

            brucieSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\BrucieCarLocations.ini", IVGame.GameStartupPath));
            brucieSettings.Load();

            string[] sectionNames = brucieSettings.GetSectionNames();

            for (int i = 0; i < sectionNames.Count(); i++)
            {
                LocationData newLocation = new LocationData();

                newLocation.Location = brucieSettings.GetVector3(sectionNames[i], "Position", Vector3.Zero);
                newLocation.Heading = brucieSettings.GetFloat(sectionNames[i], "Heading", 0);
                newLocation.Distance = 9999;

                locationList.Add(newLocation);
            }
        }
        public static void Uninit()
        {
            if (DOES_VEHICLE_EXIST(brucieCar) && IS_CAR_A_MISSION_CAR(brucieCar))
            {
                DELETE_CAR(ref brucieCar);
            }
            REMOVE_BLIP(carBlip);
        }
        public static void GameLoad()
        {
            IVText.TheIVText.ReplaceTextOfTextLabel(0x539F32DE, "Vehicle");
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("brucie_heli"))
            {
                NativeGame.TerminateScriptsWithThisName("brucie_heli");
                spawnTheCar = true;
            }
            if (!DOES_VEHICLE_EXIST(brucieCar) && spawnTheCar)
            {
                Vector3 carCoord = Vector3.Zero;
                for (int i = 0; i < locationList.Count; i++)
                {
                    GET_DISTANCE_BETWEEN_COORDS_3D(Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, locationList[i].Location.X, locationList[i].Location.Y, locationList[i].Location.Z, out float pDist);
                    /*if (pDist >= 50)
                        spawnLocList.Add(pDist);

                    else
                        spawnLocList.Add(9999);*/
                    locationList[i].Distance = pDist;
                }

                //carCoord = locationList[spawnLocList.IndexOf(spawnLocList.Min())].Location;
                //carCoord = locationList.Where(v => v.Distance).First().DownForce);

                /*float carDist = locationList.Aggregate(50.0f, (closest, next) => closest < next.Distance ? closest : next.Distance);

                carCoord = locationList.Find(x => x.Distance == carDist).Location;*/

                //IVGame.ShowSubtitleMessage(carDist.ToString());
                float carDist = locationList.Aggregate(9999.0f, (currDist, nextDist) => ((currDist < nextDist.Distance && currDist >= 50) || nextDist.Distance < 50) ? currDist : nextDist.Distance);

                carCoord = locationList.Find(x => x.Distance == carDist).Location;
                float carHdng = locationList.Find(x => x.Distance == carDist).Heading;

                int carModelIndex = GENERATE_RANDOM_INT_IN_RANGE(0, carList.Count);

                SpawnBrucieCar(carList[carModelIndex], carCoord, carHdng);
                GET_GAME_TIMER(out fTimer);

                //spawnLocList.Clear();
            }

            if (DOES_VEHICLE_EXIST(brucieCar) && IS_CAR_A_MISSION_CAR(brucieCar))
            {
                GET_CAR_COORDINATES(brucieCar, out Vector3 carCoords);
                if (!LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, carCoords.X, carCoords.Y, carCoords.Z, 50, 50, 50, false))
                {
                    if (Main.gTimer >= fTimer + timeLimit)
                    {
                        DELETE_CAR(ref brucieCar);
                        REMOVE_BLIP(carBlip);
                    }
                }
                else
                {
                    if (IS_CHAR_IN_CAR(Main.PlayerHandle, brucieCar))
                    {
                        int slots = 0;
                        float mult = 0;

                        for (int i = 1; i < 10; i++)
                        {
                            bool canGiveWeapInThisSlot = false;

                            for (int w = 0; w < weapList.Count; w++)
                            {
                                GET_WEAPONTYPE_SLOT(weapList[w], out int slot);
                                if (slot == i)
                                {
                                    giveWeaponList.Add(w);
                                    canGiveWeapInThisSlot = true;
                                }
                            }

                            if (canGiveWeapInThisSlot)
                            {
                                slots += 1;
                                GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, i, out int pWeap, out int pAmmo0, out int pAmmo1);
                                if (pWeap > 0)
                                {
                                    GET_MAX_AMMO_IN_CLIP(Main.PlayerHandle, pWeap, out int clipAmmo);
                                    GET_MAX_AMMO(Main.PlayerHandle, pWeap, out int maxAmmo);

                                    ADD_AMMO_TO_CHAR(Main.PlayerHandle, pWeap, clipAmmo * 2);

                                    if (pAmmo0 > (maxAmmo - (clipAmmo * 2)))
                                    {
                                        mult += ((float)(pAmmo0 - (maxAmmo - (clipAmmo * 2)))) / ((float)clipAmmo * 2);

                                        //IVGame.ShowSubtitleMessage(mult.ToString() + "  " + pAmmo0.ToString() + "  " + pAmmo1.ToString());
                                    }
                                }
                                else
                                {
                                    int randWeap = GENERATE_RANDOM_INT_IN_RANGE(0, giveWeaponList.Count());

                                    int clipSize = (IVWeaponInfo.GetWeaponInfo((uint)giveWeaponList[randWeap]).ClipSize) * 2;
                                    GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, giveWeaponList[randWeap], clipSize, false);
                                }
                            }
                            giveWeaponList.Clear();
                        }
                        int moneyToGive = GENERATE_RANDOM_INT_IN_RANGE(baseMoneyMin, baseMoneyMax + 1);

                        ADD_SCORE(Main.PlayerIndex, (int)(extraMoneyAmt * (mult / slots)) + moneyToGive);

                        ADD_ARMOUR_TO_CHAR(Main.PlayerHandle, armorAmt);

                        REMOVE_BLIP(carBlip);
                        MARK_CAR_AS_NO_LONGER_NEEDED(brucieCar);
                    }
                }
            }
        }
        private static void SpawnBrucieCar(string carModel, Vector3 pos, float heading)
        {
            if (!HAS_MODEL_LOADED(GET_HASH_KEY(carModel)))
                REQUEST_MODEL(GET_HASH_KEY(carModel));

            else
            {
                CREATE_CAR(GET_HASH_KEY(carModel), pos, out brucieCar, true);
                SET_CAR_HEADING(brucieCar, heading);

                LOCK_CAR_DOORS(brucieCar, 7);

                ADD_BLIP_FOR_CAR(brucieCar, out carBlip);

                NativeBlip pBlip = new NativeBlip(carBlip);

                pBlip.Icon = BlipIcon.Building_Garage;
                pBlip.Name = "Exotic Car";
                pBlip.Scale = 1.0f;
                pBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;
                pBlip.ShowOnlyWhenNear = false;
                spawnTheCar = false;
            }
        }
    }
    public class LocationData
    {
        public Vector3 Location { get; set; }
        public float Heading { get; set; }
        public float Distance { get; set; }
    }
}
