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
    internal class AlexAbility
    {
        // IniShit
        private static float alexStat;
        private static uint discountAmt;

        // OtherShit
        private static bool hasDeathArrest;
        private static uint currMoney;

        public static void Init(SettingsFile settings)
        {
            alexStat = settings.GetFloat("LIBERATED MAN", "LikeRequirement", 80);
            discountAmt = settings.GetUInteger("LIBERATED MAN", "DiscountPercent", 25);
        }
        public static void Tick()
        {
            if (GET_FLOAT_STAT(32) > alexStat)
            {
                STORE_SCORE(Main.PlayerIndex, out uint pMoney);

                if ((IS_CHAR_DEAD(Main.PlayerHandle) || IS_PLAYER_BEING_ARRESTED()) && IS_SCREEN_FADING_OUT())
                {
                    STORE_SCORE(Main.PlayerIndex, out currMoney);
                }
                if ((GET_TIME_SINCE_LAST_ARREST() < 1000 || GET_TIME_SINCE_LAST_DEATH() < 1000) && !hasDeathArrest && currMoney > pMoney)
                {
                    ADD_SCORE(Main.PlayerIndex, (int)((float)(currMoney - pMoney) * ((float)discountAmt / 100)));
                    hasDeathArrest = true;
                }
                else if (GET_TIME_SINCE_LAST_ARREST() > 1000 && GET_TIME_SINCE_LAST_DEATH() > 1000)
                    hasDeathArrest = false;
            }
        }
    }
}
