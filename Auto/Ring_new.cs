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
        private Point BattleButton, OpButton, DoneButton, SpeedButton, ConfirmButton, XButton, ComeBackButton, InitialStartButton,TaskbarButton, PvPButton;
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
            //check
            XButton = new Point(1350 + AppLocation.X, 110 + AppLocation.Y);
            ComeBackButton = new Point(50 + AppLocation.X, 80 + AppLocation.Y);
            InitialStartButton = new Point(703 + AppLocation.X, 707 + AppLocation.Y);
            TaskbarButton = new Point(1344 + AppLocation.X, 770 + AppLocation.Y);
            PvPButton = new Point(880 + AppLocation.X, 770 + AppLocation.Y);

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
                CheckMessageToBackup();
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
                if (i % 100 == 50)
                    com.SendMessage("AutoRing_new. Finish loop: " + i.ToString());
                if (i % 10 == 9)
                {
                    main.Log("AutoRing_new. Saved in loop: " + i.ToString());
                    com.SwitchTab();  //Save ring for every 10 loops
                }
            }
        }

        public void CheckMessageToBackup()
        {
            if (main.IsStopped) return;
            main.Log("CheckMessageToBackup. Begin");
            try
            {
                if (com.CheckBackupMessage())
                {
                    com.SendMessage("Stopped. Start backup...........");
                    WinAPI.LeftClick(XButton, 4000);
                    WinAPI.LeftClick(ComeBackButton, 2500);
                    com.Backup();
                    WinAPI.LeftClick(InitialStartButton, 2500);
                    //Check pvp bar
                    WinAPI.LeftClick(TaskbarButton, 1500);
                    for (int i = 0; i < 10; i++)
                        WinAPI.LeftClick(PvPButton, 500);
                    com.SendMessage("Backup done. Continue autoRing_new!");
                }
            }
            catch (Exception ex)
            {
                main.Log("CheckMessageToBackup. Ex: " + ex.Message.ToString());
            }
            return;
        }

    }
}
