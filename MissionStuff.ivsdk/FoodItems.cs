using MissionStuff.ivsdk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionStuff.ivsdk
{
    internal class FoodItems
    {
        private static SimpleMenu foodMenu;
        public static List<FoodData> foodData = new List<FoodData>();
        public static void InitMenu()
        {
            foodMenu = new SimpleMenu("Snacks");

            string menuName = "Snacks";
            Action menuAction = foodMenu.Show;
            Func<string> menuDescription = () => "Consume snacks and other food items.";

            Main.actionMenu.AddItem(menuName, menuAction, menuDescription, null, null);

            for (int i = 0; i < foodData.Count; i++)
            {
                string itemName = foodData[i].Name;
                string itemDesc = foodData[i].Desc;

                menuName = itemName;
                menuAction = ConsumeItem;
                menuDescription = () => itemDesc;

                foodMenu.AddItem(menuName, menuAction, menuDescription, null, null);
            }
        }
        private static void ConsumeItem()
        {
        }
    }
}
