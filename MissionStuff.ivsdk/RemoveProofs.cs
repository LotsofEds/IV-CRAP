using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class RemoveProofs
    {
        private static readonly List<string> SCOList = new List<string>();
        private static MissionData[] missionData;

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("REMOVE PED PROOFS", "SCOList", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                if (!Main.scoSettings.DoesSectionExists(SCOName))
                    IVGame.Console.Print("~r~ERROR: Script name in MoreWantedStars SCOList does not have a section in SCOSettings.ini!");
                else
                    SCOList.Add(SCOName);
            }

            missionData = new MissionData[SCOList.Count];

            foreach (var scoName in SCOString.Split(','))
            {
                int i = SCOList.IndexOf(scoName);

                missionData[i] = new MissionData();

                missionData[i].BulletProof = Main.scoSettings.GetBoolean(scoName, "RPBulletproof", false);
                missionData[i].FireProof = Main.scoSettings.GetBoolean(scoName, "RPFireproof", false);
                missionData[i].ExplosionProof = Main.scoSettings.GetBoolean(scoName, "RPExplosionproof", false);
                missionData[i].CollisionProof = Main.scoSettings.GetBoolean(scoName, "RPCollisionproof", false);
                missionData[i].MeleeProof = Main.scoSettings.GetBoolean(scoName, "RPMeleeproof", false);

                string pedString = Main.scoSettings.GetValue(scoName, "RPModels", "none");
                missionData[i].ModelList = new List<string>();

                foreach (var pedModel in pedString.Split(','))
                    missionData[i].ModelList.Add(pedModel);
            }
        }
        public static void Tick()
        {
            foreach (string MissionSCO in SCOList)
            {
                if (NativeGame.IsScriptRunning(MissionSCO))
                {
                    int i = SCOList.IndexOf(MissionSCO);

                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!DOES_CHAR_EXIST(pedHandle)) continue;
                        if (!IS_PED_A_MISSION_PED(pedHandle)) continue;
                        if (pedHandle == Main.PlayerHandle) continue;
                        if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 57)) continue;

                        GET_CHAR_MODEL(pedHandle, out int pModel);

                        foreach (string pedModel in missionData[i].ModelList)
                        {
                            if (pModel == GET_HASH_KEY(pedModel) || missionData[i].ModelList[0] == "none")
                            {
                                SET_CHAR_INVINCIBLE(pedHandle, false);
                                SET_CHAR_PROOFS(pedHandle, missionData[i].BulletProof, missionData[i].FireProof, missionData[i].ExplosionProof, missionData[i].CollisionProof, missionData[i].ExplosionProof);
                                SET_CHAR_ONLY_DAMAGED_BY_PLAYER(pedHandle, false);
                            }
                        }
                    }
                }
            }
        }
        public class MissionData
        {
            public bool BulletProof { get; set; }
            public bool FireProof { get; set; }
            public bool ExplosionProof { get; set; }
            public bool CollisionProof { get; set; }
            public bool MeleeProof { get; set; }
            public List<string> ModelList { get; set; }
            public MissionData()
            {

            }
        }
    }
}
