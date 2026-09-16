using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk.Helpers
{
    public class GenHelp
    {
        public static List<int> GetWeaponInventory(bool IncludeMelee)
        {
            List<int> inventory = new List<int>();

            for (int i = 0; i <= 8; i++)
            {
                GET_CHAR_WEAPON_IN_SLOT(Main.PlayerPed.GetHandle(), i, out int weaponInSlot, out _, out _);
                if (weaponInSlot == 0) continue;

                var info = IVWeaponInfo.GetWeaponInfo((uint)weaponInSlot);
                if (info == null) continue;

                if (info.FireType != 0 || IncludeMelee)
                {
                    inventory.Add(weaponInSlot);
                }
            }

            return inventory;
        }
        public static Dictionary<int, int> GetWeaponAmmoCounts()
        {
            Dictionary<int, int> ammoCounts = new Dictionary<int, int>();

            foreach (int weapon in GetWeaponInventory(false))
            {
                GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)weapon, out int ammo);
                ammoCounts[weapon] = ammo;
            }

            return ammoCounts;
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        public static Vector3 StringToVector(string stringToParse, Vector3 defaultValue)
        {
            if (string.IsNullOrWhiteSpace(stringToParse))
                return defaultValue;

            string[] array = stringToParse.Split(':');
            if ((int)array.LongLength > 5)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                float result = 0f;
                float result2 = 0f;
                float result3 = 0f;
                CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                flag = float.TryParse(array[1], NumberStyles.Float | NumberStyles.AllowThousands, invariantCulture, out result) || flag;
                CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
                flag2 = float.TryParse(array[3], NumberStyles.Float | NumberStyles.AllowThousands, invariantCulture2, out result2) || flag2;
                CultureInfo invariantCulture3 = CultureInfo.InvariantCulture;
                flag3 = float.TryParse(array[5], NumberStyles.Float | NumberStyles.AllowThousands, invariantCulture3, out result3) || flag3;
                if (flag && flag2 && flag3)
                {
                    Vector3 result4 = new Vector3(result, result2, result3);
                    return result4;
                }
            }

            return defaultValue;
        }
        public static Vector3 QuaternionToRotation(float X, float Y, float Z, float W)
        {
            double num = W;
            double num2 = X;
            double num3 = Y;
            double num4 = Z;
            double y = ((double)Z * (double)Y + (double)X * (double)W) * 2.0;
            double num5 = num;
            double num6 = num5 * num5;
            double num7 = num2;
            double num8 = num6 - num7 * num7;
            double num9 = num3;
            double num10 = num8 - num9 * num9;
            double num11 = num4;
            float radians = (float)Math.Atan2(y, num10 + num11 * num11);
            num2 = X;
            num = W;
            num3 = Y;
            num4 = Z;
            double y2 = ((double)Y * (double)X + (double)Z * (double)W) * 2.0;
            double num12 = num2;
            double num13 = num12 * num12;
            double num14 = num;
            double num15 = num13 + num14 * num14;
            double num16 = num3;
            double num17 = num15 - num16 * num16;
            double num18 = num4;
            float radians2 = (float)Math.Atan2(y2, num17 - num18 * num18);
            float radians3 = (float)Math.Asin(((double)Z * (double)X - (double)Y * (double)W) * -2.0);
            return new Vector3(Helper.RadianToDegree(radians), Helper.RadianToDegree(radians3), Helper.RadianToDegree(radians2));
        }
    }
    public class RestaurantMenuInfo
    {
        public string[] Vendors { get; set; }
        public Color HudColorA { get; set; }
        public Color HudColorB { get; set; }
        public Color TextColor { get; set; }

        public string[] Name { get; set; }
        public int[] Price { get; set; }
        public string[] Description { get; set; }
        public bool[] IsDrink { get; set; }
        public int[] HealAmount { get; set; }
        public int[] ThirstAmount { get; set; }
        public int[] HungerAmount { get; set; }
        public string[] Picture { get; set; }
        public string[] AttachedProp { get; set; }
        public Vector3[] AttachOffset { get; set; }
        public Vector3[] AttachRotation { get; set; }
        public string[][] ExtraProps { get; set; }
        public Vector3[][] PropOffsets { get; set; }
        public Vector3[][] PropRotations { get; set; }
        public RestaurantMenuInfo()
        {
        }
    }
    public class PillData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public int Price { get; set; }
        public int MaxAmount { get; set; }
        public string ScreenFX { get; set; }
        public int Duration { get; set; }
        public float EffectMax { get; set; }
        public float EffectMin { get; set; }
        public int DoseEffect { get; set; }
        public int SideEffect { get; set; }
        public bool HasTaken { get; set; }
        public bool IsActive { get; set; }
        public int Count { get; set; }
        public PillData() { }
    }
    public class PharmData
    {
        public Vector3 Location { get; set; }
        public float Heading { get; set; }
        public int Blip { get; set; }
        public PharmData()
        {
        }
    }
}
