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
    internal class CarmenAbility
    {
        // IniShit
        private static float carmenStat;
        private static uint healthRegen;
        private static uint healthThresh;
        private static uint regenTime;
        private static bool noHealthReduce;

        // BooleShit
        private static bool healthReduct;

        // OtherShit
        private static uint healthMax = 100;
        private static uint fTimer;
        public static void Init(SettingsFile settings)
        {
            carmenStat = settings.GetFloat("CARING CARMEN", "LikeRequirement", 80);
            noHealthReduce = settings.GetBoolean("CARING CARMEN", "YesFreeHealthcare", false);
            healthRegen = settings.GetUInteger("CARING CARMEN", "HealthRegenFactor", 5);
            healthThresh = settings.GetUInteger("CARING CARMEN", "RegenThresh", 40);
            regenTime = settings.GetUInteger("CARING CARMEN", "TimePerHealthRegen", 4000);
        }
        public static void Tick()
        {
            if (GET_FLOAT_STAT(30) > carmenStat)
            {
                GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);

                if (pHealth < healthThresh + 100)
                {
                    GET_CHAR_SPEED(Main.PlayerHandle, out float plyrSpd);
                    if (plyrSpd > 1.0f)
                    {
                        healthMax = 100;
                        GET_GAME_TIMER(out fTimer);
                    }

                    else
                    {
                        if (pHealth > healthMax)
                            healthMax += healthRegen;

                        else if (pHealth < healthMax)
                        {
                            healthMax = 100;
                            if (Main.gTimer >= fTimer + regenTime)
                            {
                                SET_CHAR_HEALTH(Main.PlayerHandle, pHealth + 1);
                                GET_GAME_TIMER(out fTimer);
                            }
                        }
                        else
                            GET_GAME_TIMER(out fTimer);
                    }
                }

                if (noHealthReduce)
                {
                    if (DeathAndTaxes.reduceHealth)
                        healthReduct = true;
                    DeathAndTaxes.reduceHealth = false;
                }
            }
            else
            {
                if (healthReduct)
                    DeathAndTaxes.reduceHealth = true;
            }
        }
    }
}
