using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto
{
    public class Sell
    {
        private Point AppLocation;
        private Point Item, SellButton;
        //switch tab
        private Point HomeTab, GameTab;
        public Sell()
        {
            AppLocation = WinAPI.GetAppLocation();
            Item = new Point(800 + AppLocation.X, 230 + AppLocation.Y);
            SellButton = new Point(310 + AppLocation.X, 730 + AppLocation.Y);

            //switch tab
            HomeTab = new Point(244 + AppLocation.X, 24 + AppLocation.Y);
            GameTab = new Point(444 + AppLocation.X, 24 + AppLocation.Y);
        }
        public void AutoSell(int nTimes)
        {
            Console.WriteLine("AutoSell. Begin");
            for (int i = 0; i < nTimes; i++)
            {
                WinAPI.LeftClick(Item, 500);
                WinAPI.LeftClick(SellButton, 500);
            }
            //SwitchTab();
        }
        private void SwitchTab()  //Switch tab to save
        {
            Console.WriteLine("SwitchTab. Begin");
            try
            {
                //Console.WriteLine("SwitchTab. Click HomeTab");
                WinAPI.LeftClick(HomeTab, 5000);
                //Console.WriteLine("SwitchTab. Click GameTab");
                WinAPI.LeftClick(GameTab, 5000);

            }
            catch (Exception ex)
            {
                Console.WriteLine("SwitchTab. Ex: " + ex.Message.ToString());
            }
            //Console.WriteLine("SwitchTab. End");
        }


    }
}
