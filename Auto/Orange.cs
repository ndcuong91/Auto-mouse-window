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
    public class Orange
    {
        private Point AppLocation;

        //Opening
        private Point GatePos, StartButton, SkipButton, Speed, AutoButton, ComeBackButton, InitialStartButton;
        private Rectangle ClickRect, AutoRect, StopRect, ContinueRect;
        bool bCheckAuto;

        ProcessImage pi;
        Common com;
        Auto main;
        public Orange(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;
            //Opening
            bCheckAuto = true;
            GatePos = new Point(740 + AppLocation.X, 541 + AppLocation.Y);
            StartButton = new Point(1290 + AppLocation.X, 764 + AppLocation.Y);
            SkipButton = new Point(1278 + AppLocation.X, 752 + AppLocation.Y);
            AutoButton = new Point(1315 + AppLocation.X, 100 + AppLocation.Y);
            Speed = new Point(1042 + AppLocation.X, 95 + AppLocation.Y);
            ComeBackButton = new Point(50 + AppLocation.X, 80 + AppLocation.Y);
            InitialStartButton = new Point(700 + AppLocation.X, 750 + AppLocation.Y);

            ClickRect = new Rectangle(472 + AppLocation.X, 732 + AppLocation.Y, 90, 49);
            AutoRect = new Rectangle(1260 + AppLocation.X, 87 + AppLocation.Y, 10, 10);
            StopRect = new Rectangle(405 + AppLocation.X, 891 + AppLocation.Y, 56, 23);
            ContinueRect = new Rectangle(405 + AppLocation.X, 891 + AppLocation.Y, 76, 23);

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoOrange(int nTimes)
        {
            //com.SendMessage("Test");
            DateTime initialTime = DateTime.Now;
            main.ClearLog();
            main.Log("AutoOrange. Start time: " + initialTime.ToString("[dd_MM]H_mm_ss.fff"));
            int i;
            for (i = 0; i < nTimes; i++)
            {
                main.Log("AutoOrange. Loop number: " + i.ToString());
                if (main.IsStopped) break;
                CheckMessageToBackup();
                Opening();
                Closing(4000);
                if (i % 5 == 0)
                {
                    main.Log("AutoOrange. Saved in loop: " + i.ToString());
                    com.SwitchTab();  //Save orange for every 5 times
                }
            }
            main.Log("AutoOrange. End time: " + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff"));

            double totalMinutes = DateTime.Now.Subtract(initialTime).TotalMinutes;
            if (totalMinutes > 5)
                com.SendMessage("AutoOrange. Finish run AutoOrange for " + i.ToString() + " times");
            main.SaveLog(initialTime.ToString("[dd_MM]H_mm_ss.fff") + "_Orange.txt");
            main.Log("AutoOrange. Saved log file. ");
            //Comeback to start screen
        }

        #region Sequence
        private void Opening()
        {
            if (main.IsStopped) return;
            main.Log("Opening. Begin");
            try
            {
                for (int i = 0; i < 20; i++)
                    WinAPI.LeftClick(GatePos, 400);
                WinAPI.LeftClick(StartButton, 3000);
                WinAPI.LeftClick(SkipButton, 5000);
                if (bCheckAuto)
                {
                    CheckAuto();
                    bCheckAuto = false;
                }
                WinAPI.LeftClick(Speed, 110000);

                for (int i = 0; i < 40; i++)
                {
                    Bitmap ClickImage = WinAPI.GetRectImage(ClickRect, false);
                    bool bCheckString = pi.CheckStringFromImage(ClickImage, "click", 500, false);
                    if (bCheckString)
                        break;
                }
            }
            catch (Exception ex)
            {
                main.Log("Opening. Ex: " + ex.Message.ToString());
            }
            //main.Log("Opening. End");
        }

        private void Closing(int sleep)
        {
            if (main.IsStopped) return;
            main.Log("Closing. Begin");
            try
            {
                for (int i = 0; i < 2; i++)
                    WinAPI.LeftClick(GatePos, 200);
                Thread.Sleep(sleep);
            }
            catch (Exception ex)
            {
                main.Log("Closing. Ex: " + ex.Message.ToString());
            }
            //main.Log("Closing. End");
        }

        private void CheckAuto()
        {
            if (main.IsStopped) return;
            main.Log("CheckAuto. Begin");
            try
            {
                Bitmap autoImage = WinAPI.GetRectImage(AutoRect, false);
                Color center = autoImage.GetPixel(5, 5);

                main.Log("CheckAuto. center.B: " + center.B.ToString());
                if (center.B > 150)
                {
                    main.Log("CheckAuto. Click Auto");
                    WinAPI.LeftClick(AutoButton, 1000);
                }
            }
            catch (Exception ex)
            {
                main.Log("CheckAuto. Ex: " + ex.Message.ToString());
            }
            //main.Log("Closing. End");
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
                    WinAPI.LeftClick(ComeBackButton, 2500);
                    com.Backup();
                    WinAPI.LeftClick(InitialStartButton, 2500);
                    bCheckAuto = true;
                    com.SendMessage("Backup done. Continue autoOrange!");
                }
            }
            catch (Exception ex)
            {
                main.Log("CheckMessageToBackup. Ex: " + ex.Message.ToString());
            }
            return;
        }


        #endregion
    }
}
