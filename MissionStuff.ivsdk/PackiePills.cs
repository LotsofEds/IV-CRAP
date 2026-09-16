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
    internal class PackiePills
    {
        private static bool givePills;
        // IniShit
        public static void UnInit()
        {
        }
        public static void Init (SettingsFile settings)
        {
        }
        public static void Tick()
        {
            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PK_H04"))
            {
                givePills = true;
            }
            else
                givePills = false;
        }
    }
}
