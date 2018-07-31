using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auto
{
    public class Ring_new
    {
        private Point AppLocation;

        //Opening
        private Point BattleButton, OpButton, DoneButton, SpeedButton, ConfirmButton;
        private Rectangle ClickRect;

        ProcessImage pi;
        Common com;
        Auto main;
        public Ring_new(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;
            //Opening
            BattleButton = new Point(1003 + AppLocation.X, 552 + AppLocation.Y);
            OpButton = new Point(877 + AppLocation.X, 512 + AppLocation.Y);
            DoneButton = new Point(1202 + AppLocation.X, 591 + AppLocation.Y);
            SpeedButton = new Point(1330 + AppLocation.X, 91 + AppLocation.Y);
            ConfirmButton = new Point(703 + AppLocation.X, 707 + AppLocation.Y);
            ClickRect = new Rectangle(472 + AppLocation.X, 732 + AppLocation.Y, 90, 49);

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoRing_new(int nTimes)
        {
            DateTime initialTime = DateTime.Now;
            main.ClearLog();
            main.Log("AutoRing_new. Start time: " + initialTime.ToString("[dd_MM]H_mm_ss.fff"));
            int i;
            for (i = 0; i < nTimes; i++)
            {
                if (main.IsStopped) return;
                main.Log("AutoRing_new. Loop number: " + i.ToString());

                for (int j = 0; j < 8; j++)
                    WinAPI.LeftClick(BattleButton, 1000);
                WinAPI.LeftClick(OpButton, 500);
                WinAPI.LeftClick(DoneButton, 8000);
                WinAPI.LeftClick(SpeedButton, 20000);
                WinAPI.LeftClick(ConfirmButton, 2000);
                for (int j = 0; j < 30; j++)
                {
                    Bitmap ClickImage = WinAPI.GetRectImage(ClickRect, false);
                    bool bCheckString = pi.CheckStringFromImage(ClickImage, "click", 500, false);
                    if (bCheckString)
                        break;
                }
                if (i % 10 == 9)
                {
                    main.Log("AutoRing_new. Saved in loop: " + i.ToString());
                    com.SwitchTab();  //Save orange for every 5 times
                }
                if (i % 100 == 50)
                    com.SendMessage("AutoRing_new. finish run loop: " + i.ToString());
            }

        }

    }
}
