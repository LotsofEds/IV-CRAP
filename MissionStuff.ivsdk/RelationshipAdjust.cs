using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class RelationshipAdjust
    {
        private static int romanStat;
        public static void Init(SettingsFile settings)
        {
            romanStat = settings.GetInteger("LOWER RELATIONSHIP", "RomanStat", 60);
        }
        public static void Tick()
        {
            if (GET_FLOAT_STAT(3) < 5.0f && GET_FLOAT_STAT(1) > 1.0f)
            {
                IVTheScripts.SetGlobal(10994, (int)romanStat);
                IVTheScripts.SetGlobal(10998, (int)romanStat);
            }
            /*if (IVPhoneInfo.ThePhoneInfo.CurrentNumberInput == )
            {
                IVGame.ShowSubtitleMessage("ass");
            }*/
            //if (IVPhoneInfo.ThePhoneInfo.CurrentNumberInput != string.Empty)
            //IVGame.ShowSubtitleMessage(IVPhoneInfo.ThePhoneInfo.CurrentNumberInput.ToString() + IVPhoneInfo.ThePhoneInfo.State.ToString(), 10);

            /*if (IVPhoneInfo.ThePhoneInfo.CurrentNumberInput == "0694201337" && IVPhoneInfo.ThePhoneInfo.State == 1007)
                IVGame.ShowSubtitleMessage("Nice.", 10);*/
            // Roman
                /*IVGame.ShowSubtitleMessage(IVTheScripts.GlobalVariables[10993].ToString()
                    + "  " + IVTheScripts.GlobalVariables[10994].ToString()
                    + "  " + IVTheScripts.GlobalVariables[10997].ToString()
                    + "  " + IVTheScripts.GlobalVariables[10998].ToString());*/

                /*// Jacob
                IVGame.ShowSubtitleMessage(IVTheScripts.GlobalVariables[11077].ToString()
                    + "  " + IVTheScripts.GlobalVariables[11078].ToString()
                    + "  " + IVTheScripts.GlobalVariables[11245].ToString()
                    + "  " + IVTheScripts.GlobalVariables[11246].ToString()
                    + "  " + IVTheScripts.GlobalVariables[11249].ToString()
                    + "  " + IVTheScripts.GlobalVariables[11250].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12085].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12086].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12089].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12090].ToString());
                */
                // Packie
                /*IVGame.ShowSubtitleMessage(IVTheScripts.GlobalVariables[12085].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12086].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12089].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12090].ToString());*/
                //Kate
                /*IVGame.ShowSubtitleMessage(IVTheScripts.GlobalVariables[15440].ToString()
                    + "  " + IVTheScripts.GlobalVariables[15494].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12089].ToString()
                    + "  " + IVTheScripts.GlobalVariables[12090].ToString());*/

                // PROB THESE
                // 8501

                // NOT THESE
                //10876, 10878, 10892, 529, 530, 534, 535, 536, 537, 538, 15494, 23736, 26757

                /*if (NativeControls.IsGameKeyPressed(0, GameKey.RadarZoom) || NativeControls.IsGameKeyPressed(2, GameKey.RadarZoom))
                {
                    for (int i = 1371; i < 1374; i++)
                    {
                        IVTheScripts.SetGlobal(i, 4294967295);
                    }
                    for (int i = 1381; i < 1384; i++)
                    {
                        IVTheScripts.SetGlobal(i, 4294967295);
                    }
                    for (int i = 1391; i < 1394; i++)
                    {
                        IVTheScripts.SetGlobal(i, 4294967295);
                    }
                    for (int i = 1400; i < 1450; i++)
                    {
                        IVTheScripts.SetGlobal(i, 10);
                    }
                    IVTheScripts.SetGlobal(2682, 60);
                    IVTheScripts.SetGlobal(2860, 60);
                    IVTheScripts.SetGlobal(10519, 60);
                    IVTheScripts.SetGlobal(33866, 60);
                }*/
                /*if (NativeControls.IsGameKeyPressed(0, GameKey.RadarZoom))
                {
                    //IVTheScripts.SetGlobal(8501, 4473667);
                    IVTheScripts.SetGlobal(127, 60);
                    IVTheScripts.SetGlobal(128, 60);
                    IVTheScripts.SetGlobal(135, 60);
                    IVTheScripts.SetGlobal(136, 60);
                    IVTheScripts.SetGlobal(137, 60);
                    IVTheScripts.SetGlobal(138, 60);
                    IVTheScripts.SetGlobal(529, 60);
                    IVTheScripts.SetGlobal(530, 60);
                    IVTheScripts.SetGlobal(534, 60);
                    IVTheScripts.SetGlobal(535, 60);
                    for (int i = 536; i < 546; i++)
                    {
                        IVTheScripts.SetGlobal(i, 60);
                    }
                    for (int i = 550; i < 554; i++)
                    {
                        IVTheScripts.SetGlobal(i, 60);
                    }
                    IVTheScripts.SetGlobal(557, 60);
                    IVTheScripts.SetGlobal(558, 60);
                    IVTheScripts.SetGlobal(568, 60);
                    for (int i = 2235; i < 2238; i++)
                    {
                        IVTheScripts.SetGlobal(i, 60);
                    }
                    for (int i = 8392; i < 8396; i++)
                    {
                        IVTheScripts.SetGlobal(i, 60);
                    }
                    IVTheScripts.SetGlobal(2220, 60);
                    IVTheScripts.SetGlobal(2233, 60);
                    IVTheScripts.SetGlobal(8220, 60);

                    for (int i = 8409; i < 8414; i++)
                    {
                        IVTheScripts.SetGlobal(i, 60);
                    }
                    IVTheScripts.SetGlobal(8493, 60);
                    IVTheScripts.SetGlobal(8494, 60);
                    for (int i = 8500; i < 8509; i++)
                    {
                        IVTheScripts.SetGlobal(i, 60);
                    }
                    IVTheScripts.SetGlobal(8512, 60);
                    IVTheScripts.SetGlobal(9175, 60); 
                    IVTheScripts.SetGlobal(10378, 60);
                    IVTheScripts.SetGlobal(10387, 60);
                    IVTheScripts.SetGlobal(10396, 60);
                    IVTheScripts.SetGlobal(10441, 60);
                    IVTheScripts.SetGlobal(10639, 60);
                    IVTheScripts.SetGlobal(10666, 60);
                    IVTheScripts.SetGlobal(10675, 60);
                    IVTheScripts.SetGlobal(10702, 60);
                    IVTheScripts.SetGlobal(10711, 60);
                    IVTheScripts.SetGlobal(10783, 60);
                    IVTheScripts.SetGlobal(10792, 60);
                    IVTheScripts.SetGlobal(10801, 60);
                    IVTheScripts.SetGlobal(10855, 60);
                    IVTheScripts.SetGlobal(10862, 60);
                    IVTheScripts.SetGlobal(10876, 60);
                    IVTheScripts.SetGlobal(10878, 60);
                    IVTheScripts.SetGlobal(10892, 60);
                    IVTheScripts.SetGlobal(15440, 60);
                    IVTheScripts.SetGlobal(15494, 60);
                    IVTheScripts.SetGlobal(18761, 60);
                    IVTheScripts.SetGlobal(23736, 60);
                    IVTheScripts.SetGlobal(26757, 60);
                    IVTheScripts.SetGlobal(33861, 60);
                    IVTheScripts.SetGlobal(33862, 60);
                    IVTheScripts.SetGlobal(33864, 60);
                    IVTheScripts.SetGlobal(33865, 60);
                    IVTheScripts.SetGlobal(33866, 60);
                    IVTheScripts.SetGlobal(33867, 60);
                    IVTheScripts.SetGlobal(33998, 60);
                    IVTheScripts.SetGlobal(34279, 60);
                    IVTheScripts.SetGlobal(34282, 60);
                    IVTheScripts.SetGlobal(63989, 60);
                    IVTheScripts.SetGlobal(63990, 60);
                    for (int i = 65391; i < 65535; i++)
                    {
                        IVTheScripts.SetGlobal(i, 60);
                    }
                    IVTheScripts.SetGlobal(2860, 60);
                    IVTheScripts.SetGlobal(2682, 60);
                    IVTheScripts.SetGlobal(10519, 60);
                    IVTheScripts.SetGlobal(31437, 60);
                    IVTheScripts.SetGlobal(34546, 60);
                    IVTheScripts.SetGlobal(34806, 60);
                    IVTheScripts.SetGlobal(36066, 60);
                    IVTheScripts.SetGlobal(37046, 60);
                    IVTheScripts.SetGlobal(37806, 60);
                    IVTheScripts.SetGlobal(37846, 60);
                    IVTheScripts.SetGlobal(37866, 60);
                    IVTheScripts.SetGlobal(37886, 60);
                    IVTheScripts.SetGlobal(37906, 60);
                    IVTheScripts.SetGlobal(42649, 60);
                    IVTheScripts.SetGlobal(42689, 60);
                    IVTheScripts.SetGlobal(46953, 60);
                    IVTheScripts.SetGlobal(46993, 60);
                    IVTheScripts.SetGlobal(47013, 60);
                    IVTheScripts.SetGlobal(49094, 60);
                    IVTheScripts.SetGlobal(49114, 60);
                    IVTheScripts.SetGlobal(56099, 60);
                    IVTheScripts.SetGlobal(56119, 60);
                    IVTheScripts.SetGlobal(59441, 60);
                    IVTheScripts.SetGlobal(59534, 60);
                    IVTheScripts.SetGlobal(59614, 60);


                    IVTheScripts.SetGlobal(27321, 50);
                    IVTheScripts.SetGlobal(32657, 50);
                    IVTheScripts.SetGlobal(36326, 50);
                    IVTheScripts.SetGlobal(46472, 50);

                    //IVGame.ShowSubtitleMessage(IVTheScripts.GlobalVariables[8501].ToString());
                }*/
        }
    }
}
