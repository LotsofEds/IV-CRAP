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
    internal class ChaseExtender
    {
        // IniShit
        private static bool showHealthBar;

        // OtherShit
        private static bool missionStarted;
        private static bool buffCar;

        private static int missionCar;

        public static void Init(SettingsFile settings)
        {
            showHealthBar = settings.GetBoolean("B-BUT SCRIPTED CHASES BAAAD", "ShowHealthBar", true);
        }
        public static void UnInit()
        {
            if (DOES_VEHICLE_EXIST(missionCar))
                MARK_CAR_AS_NO_LONGER_NEEDED(missionCar);

            buffCar = false;
            missionStarted = false;
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("cia1"))
            {
                if (!missionStarted)
                    missionStarted = true;

                foreach (var veh in VehHelper.VehHandles)
                {
                    int vehHandle = veh.Value;

                    if (!IS_CAR_A_MISSION_CAR(vehHandle)) continue;

                    if (IS_PLAYBACK_GOING_ON_FOR_CAR(vehHandle) && !buffCar)
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "Car Health:");

                        TURN_OFF_VEHICLE_EXTRA(vehHandle, 5, false);
                        SET_CAR_HEALTH(vehHandle, 3000);
                        SET_ENGINE_HEALTH(vehHandle, 3000);
                        SET_PETROL_TANK_HEALTH(vehHandle, 3000);

                        missionCar = vehHandle;

                        buffCar = true;
                    }
                    if (showHealthBar && DOES_VEHICLE_EXIST(missionCar) && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                        DrawHealthBar();
                }
            }
            else if (NativeGame.IsScriptRunning("brucie1"))
            {
                if (!missionStarted)
                    missionStarted = true;

                foreach (var veh in VehHelper.VehHandles)
                {
                    int vehHandle = veh.Value;

                    if (!IS_CAR_A_MISSION_CAR(vehHandle)) continue;

                    if (IS_PLAYBACK_GOING_ON_FOR_CAR(vehHandle) && !buffCar)
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "Car Health:");

                        SET_CAR_HEALTH(vehHandle, 3000);
                        SET_ENGINE_HEALTH(vehHandle, 3000);
                        SET_PETROL_TANK_HEALTH(vehHandle, 3000);

                        missionCar = vehHandle;

                        buffCar = true;
                    }
                }
                if (DOES_VEHICLE_EXIST(missionCar) && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                {
                    CONTROL_CAR_DOOR(missionCar, (int)eVehicleDoor.VEHICLE_DOOR_TRUNK, 1, 30);
                    if (showHealthBar)
                    DrawHealthBar();
                }
            }
            else if (missionStarted)
                UnInit();
        }
        private static void DrawHealthBar()
        {
            IVVehicle ass = NativeWorld.GetVehicleInstanceFromHandle(missionCar);

            float tankHealth = ass.PetrolTankHealth / 40000;
            if (tankHealth < 0)
                tankHealth = 0;
            float barPos = 0.9f - (0.075f - tankHealth) / 2;

            //IVGame.ShowSubtitleMessage(ass.PetrolTankHealth.ToString());

            SET_TEXT_FONT(0);
            SET_TEXT_SCALE(0.3f, 0.3f);
            SET_TEXT_COLOUR(255, 255, 255, 255);
            SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
            SET_TEXT_CENTRE(true);

            DISPLAY_TEXT(0.9f, 0.35f, "TM_2_30");

            DRAW_RECT(0.9f, 0.4f, 0.075f, 0.015f, 120, 120, 120, 192);
            DRAW_RECT(barPos, 0.4f, tankHealth, 0.015f, 255, 255, 255, 255);
        }
    }
}
