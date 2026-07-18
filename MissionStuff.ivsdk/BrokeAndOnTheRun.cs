using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class BrokeAndOnTheRun
    {
        // IniShit
        private static bool setMoney;
        private static int moneyRemain;
        private static int moneyLost;
        private static bool loseRomanLike;
        private static int likenessLost;

        // OtherShit
        private static bool loseMoney;

        public static void Init(SettingsFile settings)
        {
            setMoney = settings.GetBoolean("NIKO'S SORROW", "SetMoneyRemaining", false);
            moneyRemain = settings.GetInteger("NIKO'S SORROW", "MoneyRemaining", 25);
            moneyLost = settings.GetInteger("NIKO'S SORROW", "MoneyLost", 10000);
            loseRomanLike = settings.GetBoolean("NIKO'S SORROW", "LoseRomanLikenessStat", false);
            likenessLost = settings.GetInteger("NIKO'S SORROW", "StatDecrease", 17);
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("roman11"))
            {
                if (!loseMoney && IS_THIS_PRINT_BEING_DISPLAYED("R11003", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                {
                    STORE_SCORE(Main.PlayerIndex, out uint pMoney);
                    if (setMoney)
                        ADD_SCORE(Main.PlayerIndex, (moneyRemain - (int)pMoney));
                    else
                        ADD_SCORE(Main.PlayerIndex, - moneyLost);
                    loseMoney = true;
                }
                if (loseRomanLike)
                {
                    float romanLike = GET_FLOAT_STAT(1);
                    IVTheScripts.SetGlobal(10994, (int)(romanLike - likenessLost));
                    IVTheScripts.SetGlobal(10998, (int)(romanLike - likenessLost));
                }

                //IVGame.ShowSubtitleMessage((romanLike - 15).ToString() + IVTheScripts.GlobalVariables[10994].ToString() + IVTheScripts.GlobalVariables[10998].ToString());
            }
            else if (loseMoney)
                loseMoney = false;
        }
    }
}
