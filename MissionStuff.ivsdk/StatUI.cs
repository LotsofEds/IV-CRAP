using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class StatUI
    {
        public static void Tick()
        {
            if (NativeControls.IsGameKeyPressed(0, GameKey.RadarZoom) && SimpleMenu.CurrentMenu == null)
            {
                float boxSize = 0;
                float boxPos = 0.70f;
                float sleepOff = 0.70f;
                float thirstOff = 0.685f;
                float pillOff = 0.70f;

                if (Main.sleepEnable)
                {
                    boxSize += 0.03f;
                    thirstOff -= 0.015f;
                    pillOff -= 0.015f;
                }
                if (Main.thirstHungerEnable)
                {
                    boxSize += 0.06f;
                    boxPos -= 0.015f;
                    sleepOff += 0.015f;
                    pillOff -= 0.045f;
                }
                if (Main.pillsEnable)
                {
                    boxSize += 0.03f;
                    sleepOff += 0.015f;
                    thirstOff += 0.015f;
                }
                if (boxSize > 0)
                    DRAW_RECT(0.108f, boxPos, 0.15f, boxSize, 0, 0, 0, 120);

                if (Main.sleepEnable)
                {
                    float sleepSize = Main.sleepMeter / 10000.0f;
                    float sleepPos = 0.13f - (0.1f - sleepSize) / 2;

                    DRAW_RECT(0.13f, sleepOff, 0.1f, 0.01f, 100, 100, 100, 200);
                    DRAW_RECT(sleepPos, sleepOff, sleepSize, 0.01f, 255, 255, 255, 255);
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_6_1", "Energy:");

                    SET_TEXT_SCALE(0.25f, 0.25f);
                    SET_TEXT_FONT(6);
                    SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
                    SET_TEXT_CENTRE(true);

                    DISPLAY_TEXT(0.055f, sleepOff - 0.0075f, "TM_6_1");
                }
                if (Main.thirstHungerEnable)
                {
                    float thirstSize = Main.thirstMeter / 10000.0f;
                    float thirstPos = 0.13f - (0.1f - thirstSize) / 2;

                    DRAW_RECT(0.13f, thirstOff - 0.015f, 0.1f, 0.01f, 100, 100, 100, 200);
                    DRAW_RECT(thirstPos, thirstOff - 0.015f, thirstSize, 0.01f, 255, 255, 255, 255);

                    float hungerSize = Main.hungerMeter / 10000.0f;
                    float hungerPos = 0.13f - (0.1f - hungerSize) / 2;

                    DRAW_RECT(0.13f, thirstOff + 0.015f, 0.1f, 0.01f, 100, 100, 100, 200);
                    DRAW_RECT(hungerPos, thirstOff + 0.015f, hungerSize, 0.01f, 255, 255, 255, 255);

                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_5_4", "Water:");
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_5_5", "Food:");

                    SET_TEXT_SCALE(0.25f, 0.25f);
                    SET_TEXT_FONT(6);
                    SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
                    SET_TEXT_CENTRE(true);

                    DISPLAY_TEXT(0.055f, thirstOff - 0.0225f, "TM_5_4");
                    USE_PREVIOUS_FONT_SETTINGS();
                    DISPLAY_TEXT(0.055f, thirstOff + 0.0075f, "TM_5_5");
                }
                if (Main.pillsEnable)
                {
                    float pillSize = Main.pillMeter / 10000.0f;
                    float pillPos = 0.13f - (0.1f - pillSize) / 2;

                    DRAW_RECT(0.13f, pillOff, 0.1f, 0.01f, 100, 100, 100, 200);
                    DRAW_RECT(pillPos, pillOff, pillSize, 0.01f, 255, 255, 255, 255);
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_6_2", "Dosage:");

                    SET_TEXT_SCALE(0.25f, 0.25f);
                    SET_TEXT_FONT(6);
                    SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
                    SET_TEXT_CENTRE(true);

                    DISPLAY_TEXT(0.055f, pillOff - 0.0075f, "TM_6_2");
                }
            }
        }
    }
}
