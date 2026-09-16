using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Linq;
using static IVSDKDotNet.Native.Natives;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace MissionStuff.ivsdk
{
    // Player can take pills using the menu.
    // If player takes pills, increase "dosage" meter by certain amount, depending on pill.
    // If dosage meter is full, make player lose health and die (overdose).
    // Additionally, higher dosage meter will lower pill effectiveness.

    internal class Pills
    {
        // IniShit
        private static int doseTick;
        private static int doseCooldown;

        // SettingFiles
        private static SettingsFile pillSettings;
        private static SettingsFile pharmacySettings;

        // BooleShit
        private static bool pillActive;
        private static bool gotHealth;
        private static bool gotPillCount;

        // OtherShit
        private static int pillDuration;
        private static uint fTimer;
        private static int objHandle;
        private static uint oldHealth;
        private static bool inMenu;
        private static int pillIndex;

        public static List<PillData> pillData = new List<PillData>();
        private static List<PharmData> pharmData = new List<PharmData>();

        private static SimpleMenu pillMenu;

        // StatsShit
        private static uint foodTime;
        private static uint thirstTime;
        private static uint sleepTime;
        private static void DisplayDoseText()
        {
            IVText.TheIVText.ReplaceTextOfTextLabel("TM_1_6", "~s~Taking pills too often will lower its effectiveness.");

            if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_1_6"))
                PRINT_HELP("TM_1_6");
        }
        public static void UnInit()
        {
            DELETE_OBJECT(ref objHandle);
            CLEAR_TIMECYCLE_MODIFIER();
            SET_TIME_SCALE(1.0f);
            SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, 1f);

            for (int i = 0; i < pharmData.Count(); i++)
            {
                REMOVE_BLIP(pharmData[i].Blip);
            }
            pillData.Clear();
            pharmData.Clear();
            CLEAR_HELP();
            SET_PLAYER_CONTROL(Main.PlayerIndex, true);
        }
        public static void IngameStart()
        {
            fTimer = 0;
            Main.pillTimer = 0;
            gotPillCount = false;
        }
        private static void LoadPillSettings()
        {
            pillSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\Pills.ini", IVGame.GameStartupPath));
            pillSettings.Load();

            string[] sectionNames = pillSettings.GetSectionNames();

            for (int i = 0; i < sectionNames.Count(); i++)
            {
                PillData newPill = new PillData();

                newPill.ID = sectionNames[i];
                newPill.Name = pillSettings.GetValue(sectionNames[i], "Name", "");
                newPill.Desc = pillSettings.GetValue(sectionNames[i], "Description", "");
                newPill.Price = pillSettings.GetInteger(sectionNames[i], "Price", 0);
                newPill.MaxAmount = pillSettings.GetInteger(sectionNames[i], "MaxAmount", 0);
                newPill.Duration = pillSettings.GetInteger(sectionNames[i], "Duration", 0);
                newPill.EffectMax = pillSettings.GetFloat(sectionNames[i], "PillEffectMax", 0);
                newPill.EffectMin = pillSettings.GetFloat(sectionNames[i], "PillEffectMin", 0);
                newPill.DoseEffect = pillSettings.GetInteger(sectionNames[i], "DosePenalty", 0);
                newPill.SideEffect = pillSettings.GetInteger(sectionNames[i], "SideEffect", 0);
                newPill.ScreenFX = pillSettings.GetValue(sectionNames[i], "ScreenFx", "");
                newPill.HasTaken = false;
                newPill.IsActive = false;

                pillData.Add(newPill);
            }
        }
        private static void LoadPharmSettings()
        {
            pharmacySettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\Pharmacies.ini", IVGame.GameStartupPath));
            pharmacySettings.Load();

            string[] sectionNames = pharmacySettings.GetSectionNames();

            for (int i = 0; i < sectionNames.Count(); i++)
            {
                PharmData newPharm = new PharmData();

                newPharm.Location = pharmacySettings.GetVector3(sectionNames[i], "Location", Vector3.Zero);
                newPharm.Heading = pharmacySettings.GetFloat(sectionNames[i], "Heading", 0);
                newPharm.Blip = -1;

                pharmData.Add(newPharm);
            }
        }
        public static void Init(SettingsFile settings)
        {
            LoadPillSettings();
            LoadPharmSettings();

            doseTick = settings.GetInteger("PILLS", "DoseCooldownTick", 1200);
            doseCooldown = settings.GetInteger("PILLS", "InitialCooldown", 30000);

            InitMenu();
        }
        public static void InitMenu()
        {
            pillMenu = new SimpleMenu("Pills");

            string menuName = "Pills";
            Action menuAction = pillMenu.Show;
            Func<string> menuDescription = () => "Take performance-enhancing pills.";

            Main.actionMenu.AddItem(menuName, menuAction, menuDescription, null, null);

            for (int i = 0; i < pillData.Count; i++)
            {
                string pillName = pillData[i].Name;
                string pillDesc = pillData[i].Desc;

                menuName = pillName;
                menuAction = TakePill;
                menuDescription = () => pillDesc;

                pillMenu.AddItem(menuName, menuAction, menuDescription, null, null);
            }
        }
        public static void SavePillCount(SettingsFile settings)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);

            for (int i = 0; i < pillData.Count; i++)
            {
                if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, pillData[i].ID + "Count"))
                    settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, pillData[i].ID + "Count");

                settings.SetInteger(IVGenericGameStorage.ValidSaveName, pillData[i].ID + "Count", pillData[i].Count);
            }

            settings.Save();
            settings.Load();
        }
        private static void GetPillCountAtGameStart()
        {
            if (!gotPillCount)
            {
                for (int i = 0; i < pillData.Count; i++)
                {
                    if (!Main.savefileSettings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, pillData[i].ID + "Count"))
                        Main.savefileSettings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, pillData[i].ID + "Count");

                    pillData[i].Count = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, pillData[i].ID + "Count", 0);
                }
                for (int i = 0; i < pharmData.Count(); i++)
                    REMOVE_BLIP(pharmData[i].Blip);

                gotPillCount = true;
            }
        }
        private static void LoadAnimAndModel()
        {
            if (!HAVE_ANIMS_LOADED("amb@sprunk_plyr"))
                REQUEST_ANIMS("amb@sprunk_plyr");
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("cspillbottle")))
                REQUEST_MODEL(GET_HASH_KEY("cspillbottle"));
        }
        public static void Tick()
        {
            GetPillCountAtGameStart();
            
            // Update pill counts in the menu shit and request assets to use.
            if (pillMenu.IsActive)
            {
                LoadAnimAndModel();

                for (int i = 0; i < pillMenu._items.Count; i++)
                    pillMenu._items[i].Label = pillData[i].Name + " - " + pillData[i].Count.ToString() + "x";
            }
            CreateBlip();
            ProcessBuying();

            Main.pillMeter = (int)GenHelp.Clamp(Main.pillMeter, 0, 1000);

            for (int i = 0; i < pillData.Count; i++)
            {
                if (pillData[i].HasTaken)
                {
                    GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink", out float animTime);
                    if (animTime > 0.85f || !IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                    {
                        foodTime = ThirstHunger.hungerTime;
                        thirstTime = ThirstHunger.thirstTime;
                        sleepTime = Insomnia.sleepTime;

                        GET_GAME_TIMER(out fTimer);
                        DELETE_OBJECT(ref objHandle);

                        pillDuration = pillData[i].Duration;

                        SET_TIMECYCLE_MODIFIER(pillData[i].ScreenFX);

                        pillData[i].IsActive = true;
                    }
                }
                if (pillData[i].IsActive)
                {
                    switch (i)
                    {
                        case 0:
                            ProcessPainkiller();
                            break;
                        case 1:
                            ProcessAdrenaline();
                            break;
                        case 2:
                            ProcessSteroid();
                            break;
                        case 3:
                            ProcessCaffeine();
                            break;
                        case 4:
                            ProcessFood();
                            break;
                        case 5:
                            ProcessWater();
                            break;
                    }
                    if (pillData[i].HasTaken)
                    {
                        Main.pillMeter += pillData[i].DoseEffect;

                        pillData[i].HasTaken = false;
                    }
                }
            }
            // Deplete dosage meter over time.
            if (Main.gTimer >= Main.pillTimer + doseTick && Main.gTimer >= fTimer + doseCooldown && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                Main.pillMeter--;
                GET_GAME_TIMER(out Main.pillTimer);
            }
            // Process menu shit.
            pillMenu.Tick();
        }
        // For buying shit.
        private static void CreateBlip()
        {
            for (int i = 0; i < pharmData.Count(); i++)
            {
                if (!DOES_BLIP_EXIST(pharmData[i].Blip))
                {
                    ADD_BLIP_FOR_COORD(pharmData[i].Location.X, pharmData[i].Location.Y, pharmData[i].Location.Z, out int pBlip);
                    CHANGE_BLIP_SPRITE(pBlip, 108);
                    CHANGE_BLIP_NAME_FROM_ASCII(pBlip, "Pharmacy");

                    NativeBlip currBlip = new NativeBlip(pBlip);
                    currBlip.Display = eBlipDisplay.BLIP_DISPLAY_HIDDEN;
                    currBlip.ShowOnlyWhenNear = true;
                    pharmData[i].Blip = pBlip;
                }
                else
                {
                    uint blipIsland = GET_MAP_AREA_FROM_COORDS(pharmData[i].Location.X, pharmData[i].Location.Y, pharmData[i].Location.Z);
                    
                    if (blipIsland == 2 && (IVGame.CurrentEpisode != 0 || GET_FLOAT_STAT(10) > 40))
                        CHANGE_BLIP_DISPLAY(pharmData[i].Blip, (int)eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP);
                    else if (blipIsland == 3 && (IVGame.CurrentEpisode != 0 || GET_FLOAT_STAT(22) > 80))
                        CHANGE_BLIP_DISPLAY(pharmData[i].Blip, (int)eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP);
                    else if (blipIsland < 2 && (IVGame.CurrentEpisode != 0 || GET_FLOAT_STAT(3) > 10))
                        CHANGE_BLIP_DISPLAY(pharmData[i].Blip, (int)eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP);
                }
            }
        }
        private static void ProcessBuying()
        {
            bool isAtPharm = false;
            for (int i = 0; i < pharmData.Count(); i++)
            {
                if (LOCATE_CHAR_ON_FOOT_3D(Main.PlayerHandle, pharmData[i].Location.X, pharmData[i].Location.Y, pharmData[i].Location.Z, 1.0f, 1.0f, 1.0f, true) || inMenu)
                {
                    isAtPharm = true;
                    if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_20") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_1_5") && !inMenu)
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_20", "~s~Press ~INPUT_PICKUP~ to buy pills.");
                        PRINT_HELP_FOREVER("TM_2_20");
                    }
                    if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.Action) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.Action))
                    {
                        pillIndex = 0;
                        inMenu = !inMenu;
                        if (inMenu)
                        {
                            CLEAR_HELP();
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                        }
                        else
                        {
                            CLEAR_HELP();
                            SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                        }
                    }

                    if (inMenu)
                    {
                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_1_5") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_20") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_21") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_22"))
                        {
                            IVText.TheIVText.ReplaceTextOfTextLabel("TM_1_5", "~s~Use ~PAD_LEFT~ and ~PAD_RIGHT~ to browse pills. ~n~~s~Press ~INPUT_PICKUP~ to cancel. ~n~~s~Press ~INPUT_FRONTEND_ACCEPT~ to buy. ~n~~g~" + pillData[pillIndex].Name + " $" + pillData[pillIndex].Price.ToString() + " ~n~~s~Currently have " + pillData[pillIndex].Count.ToString());
                            PRINT_HELP_FOREVER("TM_1_5");
                        }

                        if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavEnter) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavEnter))
                        {
                            STORE_SCORE(Main.PlayerIndex, out uint pMoney);
                            if (pMoney >= pillData[pillIndex].Price && pillData[pillIndex].Count < pillData[pillIndex].MaxAmount)
                            {
                                ADD_SCORE(Main.PlayerIndex, -pillData[pillIndex].Price);
                                pillData[pillIndex].Count++;
                            }
                            else if (pillData[pillIndex].Count >= pillData[pillIndex].MaxAmount)
                            {
                                CLEAR_HELP();
                                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_21", "~s~~r~You cannot carry any more pills!");
                                PRINT_HELP("TM_2_21");
                            }
                            else if (pMoney < pillData[pillIndex].Price)
                            {
                                CLEAR_HELP();
                                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_22", "~s~~r~You don't have enough money!");
                                PRINT_HELP("TM_2_22");
                            }
                        }

                        else if (IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavLeft))
                        {
                            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_21") || IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_22"))
                                CLEAR_HELP();
                            if (pillIndex > 0)
                                pillIndex--;
                            else
                                pillIndex = pillData.Count - 1;

                            CLEAR_HELP();
                        }

                        else if (IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavRight))
                        {
                            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_21") || IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_22"))
                                CLEAR_HELP();
                            if (pillIndex < pillData.Count - 1)
                                pillIndex++;
                            else
                                pillIndex = 0;

                            CLEAR_HELP();
                        }
                    }
                    break;
                }
            }
            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_20") && !isAtPharm)
                CLEAR_HELP();
        }

        // For taking the fucking pills.
        private static void TakePill()
        {
            if (!pillActive && pillData[pillMenu._selectedIndex].Count > 0)
            {
                pillActive = true;
                CREATE_OBJECT(GET_HASH_KEY("cspillbottle"), Vector3.Zero, out objHandle, true);
                //SET_OBJECT_COLLISION(objHandle, false);
                ATTACH_OBJECT_TO_PED(objHandle, Main.PlayerHandle, (uint)eBone.BONE_RIGHT_HAND, 0.1f, 0.02f, -0.02f, 0f, 0f, 0f, 0);
                _TASK_PLAY_ANIM_SECONDARY_NO_INTERRUPT(Main.PlayerHandle, "partial_drink", "amb@sprunk_plyr", 4, 0, 0, 0, 0, -1);

                pillData[pillMenu._selectedIndex].Count--;
                DisplayDoseText();
                pillData[pillMenu._selectedIndex].HasTaken = true;
            }
        }
        private static void ProcessSideEffects(int i)
        {
            switch (i)
            {
                case 0:
                    Main.sleepMeter -= 50;
                    break;
                case 1:
                    ThirstHunger.hungerTime = (uint)(foodTime / pillData[i].SideEffect);
                    ThirstHunger.thirstTime = (uint)(thirstTime / pillData[i].SideEffect);
                    Insomnia.sleepTime = (uint)(sleepTime / pillData[i].SideEffect);
                    break;
                case 2:
                    ThirstHunger.hungerTime = (uint)(foodTime / pillData[i].SideEffect);
                    break;
            }
        }
        private static void ClearPillEffects(int i)
        {
            ThirstHunger.hungerTime = foodTime;
            ThirstHunger.thirstTime = thirstTime;
            Insomnia.sleepTime = sleepTime;

            CLEAR_TIMECYCLE_MODIFIER();
            pillData[i].IsActive = false;
            pillActive = false;
        }
        private static float CalcPillEffectiveness(int pillIndex)
        {
            float pillEffect = pillData[pillIndex].EffectMax - ((pillData[pillIndex].EffectMax - pillData[pillIndex].EffectMin) * ((float)Main.pillMeter / 1000));
            return pillEffect;
        }
        private static void ProcessPainkiller()
        {
            GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);
            // If has taken any pill recently, lower effectiveness
            uint healAmt = (uint)CalcPillEffectiveness(0);
            ProcessSideEffects(0);

            SET_CHAR_HEALTH(Main.PlayerHandle, pHealth + healAmt);
            ClearPillEffects(0);
        }
        private static void ProcessAdrenaline()
        {
            // If has taken any pill recently, lower effectiveness
            float speedMult = CalcPillEffectiveness(1);

            SET_TIME_SCALE(speedMult);
            SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, (float)(1 / speedMult));
            ProcessSideEffects(1);

            if (Main.gTimer >= fTimer + pillDuration || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                SET_TIME_SCALE(1.0f);
                SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, 1.0f);

                ClearPillEffects(1);
            }
        }
        private static void ProcessSteroid()
        {
            // If has taken any pill recently, lower effectiveness
            float dmgMult = CalcPillEffectiveness(2);

            GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);
            ProcessSideEffects(2);

            if (!gotHealth)
            {
                oldHealth = pHealth;
                gotHealth = true;
            }

            if (oldHealth > pHealth && gotHealth)
            {
                SET_CHAR_HEALTH(Main.PlayerHandle, pHealth + (uint)((oldHealth - pHealth) * dmgMult));
                gotHealth = false;
            }

            if ((Main.gTimer >= fTimer + pillDuration) || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                ClearPillEffects(2);
        }
        private static void ProcessCaffeine()
        {
            // If has taken any pill recently, lower effectiveness
            int sleepAmt = (int)CalcPillEffectiveness(3);

            Main.sleepMeter += sleepAmt;
            ClearPillEffects(3);
        }
        private static void ProcessFood()
        {
            // If has taken any pill recently, lower effectiveness
            int foodAmt = (int)CalcPillEffectiveness(4);

            Main.hungerMeter += foodAmt;
            ClearPillEffects(4);
        }
        private static void ProcessWater()
        {
            // If has taken any pill recently, lower effectiveness
            int waterAmt = (int)CalcPillEffectiveness(5);

            Main.thirstMeter += waterAmt;
            ClearPillEffects(5);
        }
    }
}
