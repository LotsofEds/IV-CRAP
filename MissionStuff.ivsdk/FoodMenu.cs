using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;
using static System.Net.Mime.MediaTypeNames;

namespace MissionStuff.ivsdk
{
    internal class FoodMenu
    {
        // IniShit
        private static bool healPlayer;

        private static float animBuffer;
        private static float endBuffer;

        private static int sodaHeal;
        private static int hotdogHeal;
        private static int burgerHeal;
        private static int nutsHeal;

        private static int sodaMeter;
        private static int hotdogMeter;
        private static int burgerMeter;
        private static int nutsMeter;

        private static int sodaPrice;
        private static int hotdogPrice;
        private static int burgerPrice;
        private static int nutsPrice;

        // OtherShit
        //private static bool getSoda;
        //private static int sodasDrunk;
        private static bool buyTheFood;
        private static bool cancelOrder;
        private static bool getStats;
        private static int sodasDrunk;
        private static int hotdogsEaten;
        private static int burgersEaten;
        private static int nutsEaten;
        private static int restoEaten;

        // RestoShit
        public static List<SettingsFile> restaurants = new List<SettingsFile>();
        public static List<RestaurantMenuInfo> menuData = new List<RestaurantMenuInfo>();

        private static int restoIndex;
        private static int itemIndex;

        // OtherShit
        private static int vendorPed;
        private static int burgObj;
        //private static int burgObjB;
        private static int foodObj;
        private static List<int> propObjs = new List<int>();
        private static string vendorAnim;
        private static uint oldHealth;
        private static uint oldMoney;
        private static int tex;
        private static Vector3 clearPos;

        private static float offX = 0;
        private static float offY = 0;
        private static float offZ = 0;

        private static int rotX = 90;
        private static int rotY = 70;
        private static int rotZ = 145;

        public static void IngameStart()
        {
            getStats = false;
            restoIndex = 0;
            itemIndex = 0;
        }
        public static void UnInit()
        {
            for (int i = 0; i < propObjs.Count(); i++)
            {
                if (DOES_OBJECT_EXIST(propObjs[i]))
                {
                    int newObj = propObjs[i];
                    DELETE_OBJECT(ref newObj);
                    propObjs.RemoveAt(i);
                }
            }
            if (DOES_OBJECT_EXIST(foodObj))
                DELETE_OBJECT(ref foodObj);
            if (DOES_OBJECT_EXIST(burgObj))
                DELETE_OBJECT(ref burgObj);

            itemIndex = 0;
            vendorPed = -1;
            burgObj = -1;
            getStats = false;
            buyTheFood = false;
            cancelOrder = false;
        }
        public static void Init(SettingsFile settings)
        {
            string locPath;
            string[] locConfFiles;

            locPath = string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\MenuItems", IVGame.GameStartupPath);
            locConfFiles = System.IO.Directory.GetFiles(locPath);

            foreach (string fileName in locConfFiles)
                restaurants.Add(new SettingsFile(fileName));

            for (int i = 0; i < restaurants.Count(); i++)
            {
                restaurants[i].Load();

                menuData.Add(new RestaurantMenuInfo());

                string[] menuItems = restaurants[i].GetSectionNames();

                int numOfItems = menuItems.Count() - 1;

                string vendors = restaurants[i].GetValue("MAIN", "VendorModels", "");
                menuData[i].Vendors = vendors.Split(',').ToArray();
                menuData[i].HudColorA = restaurants[i].GetColor("MAIN", "HudColorA", Color.Black);
                menuData[i].HudColorB = restaurants[i].GetColor("MAIN", "HudColorB", Color.Black);
                menuData[i].TextColor = restaurants[i].GetColor("MAIN", "TextColor", Color.White);

                menuData[i].Name = new string[numOfItems];
                menuData[i].Price = new int[numOfItems];
                menuData[i].Description = new string[numOfItems];
                menuData[i].IsDrink = new bool[numOfItems];
                menuData[i].HealAmount = new int[numOfItems];
                menuData[i].ThirstAmount = new int[numOfItems];
                menuData[i].HungerAmount = new int[numOfItems];
                menuData[i].Picture = new string[numOfItems];
                menuData[i].AttachedProp = new string[numOfItems];
                menuData[i].AttachOffset = new Vector3[numOfItems];
                menuData[i].AttachRotation = new Vector3[numOfItems];
                menuData[i].ExtraProps = new string[numOfItems][];
                menuData[i].PropOffsets = new Vector3[numOfItems][];
                menuData[i].PropRotations = new Vector3[numOfItems][];

                for (int m = 0; m < numOfItems; m++)
                {
                    menuData[i].Name[m] = menuItems[m + 1];
                    menuData[i].Price[m] = restaurants[i].GetInteger(menuItems[m + 1], "Price", 1);
                    menuData[i].Description[m] = restaurants[i].GetValue(menuItems[m + 1], "Description", "");
                    menuData[i].IsDrink[m] = restaurants[i].GetBoolean(menuItems[m + 1], "IsDrink", false);

                    menuData[i].HealAmount[m] = restaurants[i].GetInteger(menuItems[m + 1], "HealAmount", 1);
                    menuData[i].ThirstAmount[m] = restaurants[i].GetInteger(menuItems[m + 1], "ThirstReplenish", 1);
                    menuData[i].HungerAmount[m] = restaurants[i].GetInteger(menuItems[m + 1], "HungerReplenish", 1);
                    menuData[i].Picture[m] = restaurants[i].GetValue(menuItems[m + 1], "Picture", "");

                    menuData[i].AttachedProp[m] = restaurants[i].GetValue(menuItems[m + 1], "AttachedProp", "");
                    menuData[i].AttachOffset[m] = restaurants[i].GetVector3(menuItems[m + 1], "AttachOffset", Vector3.Zero);
                    menuData[i].AttachRotation[m] = restaurants[i].GetVector3(menuItems[m + 1], "AttachRotation", Vector3.Zero);

                    string attachedPropModels = restaurants[i].GetValue(menuItems[m + 1], "ExtraProps", "");
                    string[] attachedProps = attachedPropModels.Split(',').ToArray();

                    string attachOffsets = restaurants[i].GetValue(menuItems[m + 1], "PropOffsets", "");
                    string[] attachOffs = attachOffsets.Split(',').ToArray();

                    string attachRotations = restaurants[i].GetValue(menuItems[m + 1], "PropRotations", "");
                    string[] attachRots = attachRotations.Split(',').ToArray();

                    menuData[i].ExtraProps[m] = new string[attachedProps.Count()];
                    menuData[i].PropOffsets[m] = new Vector3[attachedProps.Count()];
                    menuData[i].PropRotations[m] = new Vector3[attachedProps.Count()];

                    menuData[i].ExtraProps[m] = attachedProps;

                    for (int v = 0; v < attachOffs.Count(); v++)
                        menuData[i].PropOffsets[m][v] = GenHelp.StringToVector(attachOffs[v], Vector3.Zero);

                    for (int v = 0; v < attachRots.Count(); v++)
                        menuData[i].PropRotations[m][v] = GenHelp.StringToVector(attachRots[v], Vector3.Zero);

                    if (attachOffs.Count() < attachedProps.Count())
                    {
                        for (int v = attachOffs.Count(); v < attachedProps.Count(); v++)
                            menuData[i].PropOffsets[m][v] = Vector3.Zero;
                    }

                    if (attachRots.Count() < attachedProps.Count())
                    {
                        for (int v = attachRots.Count(); v < attachedProps.Count(); v++)
                            menuData[i].PropRotations[m][v] = Vector3.Zero;
                    }
                }
            }
            healPlayer = settings.GetBoolean("FOOD MENU", "HealThePlayer", false);

            animBuffer = settings.GetFloat("FOOD MENU", "AnimBuffer", 0.03f);
            endBuffer = settings.GetFloat("FOOD MENU", "AnimCancel", 0.98f);

            sodaHeal = settings.GetInteger("FOOD MENU", "SodaHeal", 5);
            hotdogHeal = settings.GetInteger("FOOD MENU", "HotdogHeal", 25);
            burgerHeal = settings.GetInteger("FOOD MENU", "BurgerHeal", 25);
            nutsHeal = settings.GetInteger("FOOD MENU", "NutsHeal", 25);

            sodaMeter = settings.GetInteger("FOOD MENU", "SodaMeter", 20);
            hotdogMeter = settings.GetInteger("FOOD MENU", "HotdogMeter", 20);
            burgerMeter = settings.GetInteger("FOOD MENU", "BurgerMeter", 20);
            nutsMeter = settings.GetInteger("FOOD MENU", "NutsMeter", 20);

            sodaPrice = settings.GetInteger("FOOD MENU", "SodaPrice", 1);
            hotdogPrice = settings.GetInteger("FOOD MENU", "HotdogPrice", 5);
            burgerPrice = settings.GetInteger("FOOD MENU", "BurgerPrice", 5);
            nutsPrice = settings.GetInteger("FOOD MENU", "NutsPrice", 5);

            getStats = false;
            restoIndex = 0;
            itemIndex = 0;
        }
        public static void Tick()
        {
            ProcessPriceAndHealth();
            ProcessProps();

            if (cancelOrder)
                SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@ffood_server", "eat_burger_plyr", 1.0f);

            if (!IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                bool isBuying = false;
                foreach (var ped in PedHelper.PedHandles)
                {
                    int pedHandle = ped.Value;
                    if (!DOES_CHAR_EXIST(pedHandle)) continue;
                    if (!IS_PED_A_MISSION_PED(pedHandle)) continue;
                    if (pedHandle == Main.PlayerHandle) continue;

                    GET_CHAR_COORDINATES(pedHandle, out Vector3 pos);
                    GET_DISTANCE_BETWEEN_COORDS_3D(pos.X, pos.Y, pos.Z, Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, out float dist);

                    if (dist > 10) continue;

                    GET_CHAR_MODEL(pedHandle, out int pedMdl);

                    for (int i = 0; i < menuData.Count; i++)
                    {
                        bool gotResto = false;
                        for (int v = 0; v < menuData[i].Vendors.Count(); v++)
                        {
                            if (pedMdl == GET_HASH_KEY(menuData[i].Vendors[v]))
                            {
                                restoIndex = i;
                                gotResto = true;
                                break;
                            }
                        }
                        if (gotResto)
                            break;
                    }

                    vendorPed = pedHandle;

                    if (IS_CHAR_PLAYING_ANIM(pedHandle, "amb@ffood_server", "sell_burger_male"))
                    {
                        vendorAnim = "sell_burger_male";
                        isBuying = true;
                    }

                    else if (IS_CHAR_PLAYING_ANIM(pedHandle, "amb@ffood_server", "sell_burger_female"))
                    {
                        vendorAnim = "sell_burger_female";
                        isBuying = true;
                    }
                }
                if (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr") && isBuying)
                {
                    GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr", out float animTime);
                    if (animTime < animBuffer)
                    {
                        ProcessUI();
                        ProcessControls();

                        SET_CHAR_ANIM_SPEED(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr", 0.001f);
                        SET_CHAR_ANIM_SPEED(vendorPed, "amb@ffood_server", vendorAnim, 0.001f);
                        SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr", 0.001f);
                        SET_CHAR_ANIM_CURRENT_TIME(vendorPed, "amb@ffood_server", vendorAnim, 0.001f);
                    }
                    else if (!buyTheFood && !cancelOrder)
                        CancelBuying();

                    if (IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavEnter) && !buyTheFood)
                    {
                        CLEAR_HELP();
                        if (oldMoney >= menuData[restoIndex].Price[itemIndex])
                        {
                            buyTheFood = true;
                            SET_CHAR_ANIM_SPEED(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr", 1.0f);
                            SET_CHAR_ANIM_SPEED(vendorPed, "amb@ffood_server", vendorAnim, 1.0f);
                            SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr", animBuffer);
                            SET_CHAR_ANIM_CURRENT_TIME(vendorPed, "amb@ffood_server", vendorAnim, animBuffer);

                            if (HAS_STREAMED_TXD_LOADED("menuitems"))
                                MARK_STREAMED_TXD_AS_NO_LONGER_NEEDED("menuitems");
                            if (tex > 0)
                            {
                                RELEASE_TEXTURE(tex);
                                tex = 0;
                            }
                        }
                        else
                            PRINT_HELP("FAIL_TRAN");
                    }
                    else if (!buyTheFood && !cancelOrder && (IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavBack) || oldMoney < menuData[restoIndex].Price.Min()))
                        CancelBuying();
                }
            }
        }
        private static void ProcessPriceAndHealth()
        {
            if (!getStats)
            {
                sodasDrunk = GET_INT_STAT(374);
                hotdogsEaten = GET_INT_STAT(307);
                burgersEaten = GET_INT_STAT(308);
                nutsEaten = GET_INT_STAT(309);
                restoEaten = GET_INT_STAT(306);

                if (healPlayer)
                    IVText.TheIVText.ReplaceTextOfTextLabel("HOTDOGS", "Visit fast food vendors around the city to replenish health and quell your hunger or thirst.");
                else
                    IVText.TheIVText.ReplaceTextOfTextLabel("HOTDOGS", "Visit fast food vendors around the city to quell your hunger or thirst.");

                getStats = true;
            }
            else if (getStats)
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("BUY_BLEED", "Press ~INPUT_PICKUP~ to buy.");
                IVText.TheIVText.ReplaceTextOfTextLabel("BUY_BURG", "Press ~INPUT_PICKUP~ to buy.");
                IVText.TheIVText.ReplaceTextOfTextLabel("BUY_FOWL", "Press ~INPUT_PICKUP~ to buy.");

                if (burgersEaten < GET_INT_STAT(308) && restoEaten < GET_INT_STAT(306))
                    ProcessBuying(menuData[restoIndex].HealAmount[itemIndex], menuData[restoIndex].ThirstAmount[itemIndex], menuData[restoIndex].HungerAmount[itemIndex], menuData[restoIndex].Price[itemIndex]);
                else if (sodasDrunk < GET_INT_STAT(374))
                    ProcessBuying(sodaHeal, sodaMeter, 0, sodaPrice);
                else if (hotdogsEaten < GET_INT_STAT(307))
                    ProcessBuying(hotdogHeal, 0, hotdogMeter, hotdogPrice);
                else if (burgersEaten < GET_INT_STAT(308))
                    ProcessBuying(burgerHeal, 0, burgerMeter, burgerPrice);
                else if (nutsEaten < GET_INT_STAT(309))
                    ProcessBuying(nutsHeal, 0, nutsMeter, nutsPrice);
                else
                {
                    GET_CHAR_HEALTH(Main.PlayerHandle, out oldHealth);
                    STORE_SCORE(Main.PlayerIndex, out oldMoney);
                }
            }
        }
        private static void CancelBuying()
        {
            SET_CHAR_ANIM_SPEED(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr", 1.0f);
            SET_CHAR_ANIM_SPEED(vendorPed, "amb@ffood_server", vendorAnim, 1.0f);
            SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr", endBuffer);
            SET_CHAR_ANIM_CURRENT_TIME(vendorPed, "amb@ffood_server", vendorAnim, endBuffer);

            if (HAS_STREAMED_TXD_LOADED("menuitems"))
                MARK_STREAMED_TXD_AS_NO_LONGER_NEEDED("menuitems");
            if (tex > 0)
            {
                RELEASE_TEXTURE(tex);
                tex = 0;
            }

            CLEAR_HELP();
            cancelOrder = true;
        }
        private static void ProcessControls()
        {
            if (IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavLeft))
            {
                if (itemIndex > 0)
                    itemIndex--;
                else
                    itemIndex = menuData[restoIndex].Name.Count() - 1;

                if (tex > 0)
                {
                    RELEASE_TEXTURE(tex);
                    tex = 0;
                }
            }
            else if (IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavRight))
            {
                if (itemIndex < menuData[restoIndex].Name.Count() - 1)
                    itemIndex++;
                else
                    itemIndex = 0;

                if (tex > 0)
                {
                    RELEASE_TEXTURE(tex);
                    tex = 0;
                }
            }
        }
        private static void ProcessUI()
        {
            IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_31", "Use ~PAD_LEFT~ and ~PAD_RIGHT~ to select a meal. ~n~~s~Press ~INPUT_FRONTEND_ACCEPT~ to buy. ~n~~s~Press ~INPUT_PHONE_CANCEL~ to cancel.");
            if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_31") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("FAIL_TRAN"))
                PRINT_HELP_FOREVER("TM_2_31");

            DRAW_RECT(0.85f, 0.5f, 0.3f, 1, menuData[restoIndex].HudColorB.R, menuData[restoIndex].HudColorB.G, menuData[restoIndex].HudColorB.B, menuData[restoIndex].HudColorB.A);
            DRAW_RECT(0.85f, 0.075f, 0.3f, 0.15f, menuData[restoIndex].HudColorA.R, menuData[restoIndex].HudColorA.G, menuData[restoIndex].HudColorA.B, menuData[restoIndex].HudColorA.A);

            if (tex <= 0)
            {
                if (!HAS_STREAMED_TXD_LOADED("menuitems"))
                    LOAD_TXD("menuitems");
                tex = GET_TEXTURE_FROM_STREAMED_TXD("menuitems", menuData[restoIndex].Picture[itemIndex]);
            }

            DRAW_SPRITE((uint)tex, 0.85f, 0.40f, ((float)IVGame.Resolution.Height / (float)IVGame.Resolution.Width) * 0.25f, 0.5f, 0, 255, 255, 255, 255);

            SET_TEXT_SCALE(0.6f, 0.6f);
            SET_TEXT_FONT(6);
            SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
            SET_TEXT_COLOUR(menuData[restoIndex].TextColor.R, menuData[restoIndex].TextColor.G, menuData[restoIndex].TextColor.B, 255);
            SET_TEXT_CENTRE(true);

            IVText.TheIVText.ReplaceTextOfTextLabel("GS1_ASST1_01", menuData[restoIndex].Name[itemIndex]);
            if (menuData[restoIndex].Name[itemIndex].Length < 20)
                DISPLAY_TEXT(0.85f, 0.055f, "GS1_ASST1_01");
            else
                DISPLAY_TEXT(0.85f, 0.04f, "GS1_ASST1_01");

            string helpString = "Replenishes ";
            if (healPlayer)
                helpString += menuData[restoIndex].HealAmount[itemIndex].ToString() + "% health, ";

            helpString += (menuData[restoIndex].ThirstAmount[itemIndex] / 10).ToString() + "% water, ";
            helpString += (menuData[restoIndex].HungerAmount[itemIndex] / 10).ToString() + "% food";

            USE_PREVIOUS_FONT_SETTINGS();
            SET_TEXT_SCALE(0.3f, 0.3f);
            SET_TEXT_WRAP(0.71f, 0.99f);
            SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);

            IVText.TheIVText.ReplaceTextOfTextLabel("GS1_ASST1_03", helpString);
            DISPLAY_TEXT(0.85f, 0.665f, "GS1_ASST1_03");

            USE_PREVIOUS_FONT_SETTINGS();
            IVText.TheIVText.ReplaceTextOfTextLabel("GS1_ASST2_01", "Cost: " + menuData[restoIndex].Price[itemIndex].ToString() + "$");
            DISPLAY_TEXT(0.85f, 0.735f, "GS1_ASST2_01");

            USE_PREVIOUS_FONT_SETTINGS();
            IVText.TheIVText.ReplaceTextOfTextLabel("GS1_ASST1_02", menuData[restoIndex].Description[itemIndex]);
            DISPLAY_TEXT(0.85f, 0.77f, "GS1_ASST1_02");
        }
        private static void ProcessProps()
        {
            // for debug and testing only
            /*if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.LookBehind))
            {
                REQUEST_MODEL(GET_HASH_KEY("cj_j_can2"));
                REQUEST_MODEL(GET_HASH_KEY("amb_burg"));

                if (!HAS_MODEL_LOADED(GET_HASH_KEY("cj_j_can2")))
                    return;
                if (!HAS_MODEL_LOADED(GET_HASH_KEY("amb_burg")))
                    return;

                if (!DOES_OBJECT_EXIST(foodObj))
                {
                    CREATE_OBJECT(GET_HASH_KEY("amb_burg"), Main.PlayerPos, out burgObj, false);
                    ATTACH_OBJECT_TO_PED(burgObj, Main.PlayerHandle, 1232, new Vector3(0.06f, 0.12f, -0.11f), new Vector3(1.49f, -0.31f, -0.72f), 1);

                    CREATE_OBJECT(GET_HASH_KEY("cj_j_can2"), Main.PlayerPos, out foodObj, false);
                    ATTACH_OBJECT_TO_OBJECT(foodObj, burgObj, 0, new Vector3(0.1f, 0.1f ,0), new Vector3(0f * (float)(Math.PI / 180), 90f * (float)(Math.PI / 180), -30f * (float)(Math.PI / 180)));
                    //SET_OBJECT_VISIBLE(foodObj, false);
                }
            }
            if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.RadarZoom))
            {
                if (offX > 0.2f)
                    offX = -0.2f;
                else
                    offX += 0.05f;
                rotX += 5;
            }
            if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.Action))
            {
                if (offY > 0.2f)
                    offY = -0.2f;
                else
                    offY += 0.05f;
                rotY += 5;
            }
            if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavBack))
            {
                if (offZ > 0.2f)
                    offZ = -0.2f;
                else
                    offZ += 0.05f;
                rotZ += 5;
            }

            if (DOES_OBJECT_EXIST(foodObj))
            {
                IVGame.ShowSubtitleMessage("X: " + offX.ToString() + " Y: " + offY.ToString() + " Z: " + offZ.ToString());
                ATTACH_OBJECT_TO_OBJECT(foodObj, burgObj, 0, new Vector3(offX, offY, offZ), new Vector3(rotX * (float)(Math.PI / 180), rotY * (float)(Math.PI / 180), rotZ * (float)(Math.PI / 180)));
            }*/
            foreach (var obj in ObjectHelper.ObjHandles)
            {
                int objHandle = obj.Value;
                if (!DOES_OBJECT_EXIST(objHandle)) continue;
                if (!IS_OBJECT_ATTACHED(objHandle)) continue;
                if (objHandle == foodObj) continue;

                GET_OBJECT_COORDINATES(objHandle, out Vector3 objPos);
                GET_DISTANCE_BETWEEN_COORDS_3D(objPos.X, objPos.Y, objPos.Z, Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, out float pDist);

                if (pDist > 10) continue;

                GET_OBJECT_MODEL(objHandle, out uint pModel);

                if (pModel != (uint)GET_HASH_KEY("amb_burg")) continue;

                burgObj = objHandle;
            }
            if (!DOES_OBJECT_EXIST(burgObj))
            {
                if (DOES_OBJECT_EXIST(foodObj) && IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                    ATTACH_OBJECT_TO_PED(foodObj, Main.PlayerHandle, (uint)eBone.BONE_RIGHT_HAND, menuData[restoIndex].AttachOffset[itemIndex] + new Vector3(0.08f, 0.07f, -0.06f), new Vector3((menuData[restoIndex].AttachRotation[itemIndex].X + 300) * (float)(Math.PI / 180) + 1.49f, (menuData[restoIndex].AttachRotation[itemIndex].Y) * (float)(Math.PI / 180) - 0.31f, (menuData[restoIndex].AttachRotation[itemIndex].Z + 90) * (float)(Math.PI / 180) - 0.72f), 1);
                
                //ATTACH_OBJECT_TO_OBJECT(foodObj, burgObjB, 0, menuData[restoIndex].AttachOffset[itemIndex], new Vector3(menuData[restoIndex].AttachRotation[itemIndex].X * (float)(Math.PI / 180), menuData[restoIndex].AttachRotation[itemIndex].Y * (float)(Math.PI / 180), menuData[restoIndex].AttachRotation[itemIndex].Z * (float)(Math.PI / 180)));

                //ATTACH_OBJECT_TO_PED(foodObj, Main.PlayerHandle, (uint)eBone.BONE_RIGHT_HAND, menuData[restoIndex].AttachOffset[itemIndex] + new Vector3(0.06f, 0.12f, -0.11f), new Vector3(menuData[restoIndex].AttachRotation[itemIndex].X * (float)(Math.PI / 180) + 1.49f, menuData[restoIndex].AttachRotation[itemIndex].Y * (float)(Math.PI / 180) - 0.31f, menuData[restoIndex].AttachRotation[itemIndex].Z * (float)(Math.PI / 180) - 0.72f), 1);

                else
                {
                    for (int i = 0; i < propObjs.Count(); i++)
                    {
                        if (DOES_OBJECT_EXIST(propObjs[i]))
                        {
                            int newObj = propObjs[i];
                            DELETE_OBJECT(ref newObj);
                            propObjs.RemoveAt(i);
                        }
                        CLEAR_AREA_OF_OBJECTS(clearPos, 1.0f);
                    }
                    if (DOES_OBJECT_EXIST(foodObj))
                        DELETE_OBJECT(ref foodObj);
                    //if (DOES_OBJECT_EXIST(burgObjB))
                    //DELETE_OBJECT(ref burgObjB);
                }
            }
            else if (DOES_OBJECT_EXIST(burgObj) && (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@ffood_server", "buy_burger_plyr") || IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@ffood_server", "eat_burger_plyr") || IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink")))
            {
                for (int i = 0; i < menuData[restoIndex].ExtraProps[itemIndex].Count(); i++)
                {
                    if (propObjs.Count == menuData[restoIndex].ExtraProps[itemIndex].Count()) continue;

                    int newObj = 0;
                    propObjs.Add(newObj);
                }
                for (int i = 0; i < propObjs.Count(); i++)
                {
                    if (!DOES_OBJECT_EXIST(propObjs[i]))
                    {
                        if (IS_MODEL_IN_CDIMAGE(GET_HASH_KEY(menuData[restoIndex].ExtraProps[itemIndex][i])))
                        {
                            if (!HAS_MODEL_LOADED(GET_HASH_KEY(menuData[restoIndex].ExtraProps[itemIndex][i])))
                                REQUEST_MODEL(GET_HASH_KEY(menuData[restoIndex].ExtraProps[itemIndex][i]));
                            else
                            {
                                int newObj;
                                CREATE_OBJECT(GET_HASH_KEY(menuData[restoIndex].ExtraProps[itemIndex][i]), Vector3.Zero, out newObj, false);
                                propObjs[i] = newObj;
                                MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY(menuData[restoIndex].ExtraProps[itemIndex][i]));

                                GET_KEY_FOR_CHAR_IN_ROOM(Main.PlayerHandle, out uint pKey);
                                //GET_ROOM_KEY_FROM_OBJECT(burgObj, out uint pRoomKey);
                                ADD_OBJECT_TO_INTERIOR_ROOM_BY_KEY(propObjs[i], pKey);
                                GET_OFFSET_FROM_OBJECT_IN_WORLD_COORDS(burgObj, new Vector3(0, -0.10f, 0.45f) + menuData[restoIndex].PropOffsets[itemIndex][i], out Vector3 objPos);

                                //SET_OBJECT_COLLISION(newObj, false);
                                //FREEZE_OBJECT_POSITION(newObj, true);
                                //SET_OBJECT_DYNAMIC(newObj, false);
                                clearPos = objPos;
                                CLEAR_AREA_OF_OBJECTS(objPos, 1.0f);
                                SET_OBJECT_COORDINATES(newObj, objPos);
                                GET_OBJECT_QUATERNION(burgObj, out float objX, out float objY, out float objZ, out float objW);
                                Vector3 objRot = GenHelp.QuaternionToRotation(objX, objY, objZ, objW);
                                SET_OBJECT_ROTATION(newObj, objRot + new Vector3(-20, -30, 0));
                            }
                        }
                    }
                    else
                        APPLY_FORCE_TO_OBJECT(propObjs[i], 3, new Vector3(0, 0, -0.01f * Main.frameTime), Vector3.Zero, 0, 0, 1, 1);
                }
                if (!DOES_OBJECT_EXIST(foodObj))
                {
                    if (IS_MODEL_IN_CDIMAGE(GET_HASH_KEY(menuData[restoIndex].AttachedProp[itemIndex])))
                    {
                        if (!HAS_MODEL_LOADED(GET_HASH_KEY(menuData[restoIndex].AttachedProp[itemIndex])))
                            REQUEST_MODEL(GET_HASH_KEY(menuData[restoIndex].AttachedProp[itemIndex]));
                        else
                        {
                            CREATE_OBJECT(GET_HASH_KEY(menuData[restoIndex].AttachedProp[itemIndex]), Vector3.Zero, out foodObj, false);
                            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY(menuData[restoIndex].AttachedProp[itemIndex]));
                            GET_ROOM_KEY_FROM_OBJECT(burgObj, out uint pRoomKey);
                            ADD_OBJECT_TO_INTERIOR_ROOM_BY_KEY(foodObj, pRoomKey);
                        }
                    }
                }
                else
                {
                    SET_OBJECT_VISIBLE(burgObj, false);
                    ATTACH_OBJECT_TO_OBJECT(foodObj, burgObj, 0, menuData[restoIndex].AttachOffset[itemIndex], new Vector3(menuData[restoIndex].AttachRotation[itemIndex].X * (float)(Math.PI / 180), menuData[restoIndex].AttachRotation[itemIndex].Y * (float)(Math.PI / 180), menuData[restoIndex].AttachRotation[itemIndex].Z * (float)(Math.PI / 180)));

                    if (menuData[restoIndex].IsDrink[itemIndex])
                    {
                        REQUEST_ANIMS("amb@sprunk_plyr");
                        if (IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@ffood_server", "eat_burger_plyr"))
                        {
                            //CREATE_OBJECT(GET_HASH_KEY("amb_burg"), Vector3.Zero, out burgObjB, false);
                            //SET_OBJECT_VISIBLE(burgObjB, false);
                            //ATTACH_OBJECT_TO_PED(burgObjB, Main.PlayerHandle, (uint)eBone.BONE_RIGHT_HAND, new Vector3(0.06f, 0.12f, -0.11f), new Vector3(1.49f, -0.31f, -0.72f), 1);
                            
                            _TASK_PLAY_ANIM_SECONDARY_NO_INTERRUPT(Main.PlayerHandle, "partial_drink", "amb@sprunk_plyr", 8, 0, 0, 0, 0, -1);
                            DETACH_OBJECT(foodObj, true);
                        }
                    }
                }
            }
        }
        private static void ProcessBuying(int healAmt, int thirstAmt, int hungerAmt, int price)
        {
            STORE_SCORE(Main.PlayerIndex, out uint newScore);
            ADD_SCORE(Main.PlayerIndex, (int)(oldMoney - newScore));
            if (!cancelOrder && oldMoney >= price)
            {
                ADD_SCORE(Main.PlayerIndex, -price);
                if (healPlayer)
                    SET_CHAR_HEALTH(Main.PlayerHandle, (uint)((int)oldHealth + healAmt));
                else
                    SET_CHAR_HEALTH(Main.PlayerHandle, (uint)((int)oldHealth));

                Main.thirstMeter = ((int)Main.thirstMeter + thirstAmt);
                if (thirstAmt != 0)
                    GET_GAME_TIMER(out Main.thirstTimer);
                Main.hungerMeter = ((int)Main.hungerMeter + hungerAmt);
                if (hungerAmt != 0)
                    GET_GAME_TIMER(out Main.hungerTimer);
            }
            else
                SET_CHAR_HEALTH(Main.PlayerHandle, (uint)((int)oldHealth));

            itemIndex = 0;
            vendorPed = -1;
            burgObj = -1;
            getStats = false;
            buyTheFood = false;
            cancelOrder = false;
        }
    }
}
