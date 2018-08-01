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
    public class Gold
    {
        private Point AppLocation;
        private int nTimes;
        private const int nLoopToSave = 20;

        //Opening
        private Point GatePos, StartButton, Speed;
        private Rectangle Sky, ClickRect;

        //Attack The Monk
        private Point Hunter, Immortar, Prince, Women, Lady, Target;

        //switch tab
        private Point ComeBackButton;

        ProcessImage pi;
        Common com;
        Auto main;
        public Gold(Point appLocation, Auto mainForm)
        {
            AppLocation = appLocation;
            pi = new ProcessImage(mainForm);
            //Opening
           // GatePos = new Point(709 + AppLocation.X, 726 + AppLocation.Y);
            GatePos = new Point(196 + AppLocation.X, 417 + AppLocation.Y);
            //GatePos = new Point(735 + AppLocation.X, 555 + AppLocation.Y);
            StartButton = new Point(1290 + AppLocation.X, 764 + AppLocation.Y);
            Speed = new Point(1227 + AppLocation.X, 95 + AppLocation.Y);

            //Sky image
            Sky = new Rectangle(165 + AppLocation.X, 720 + AppLocation.Y, 50, 50);
            ClickRect = new Rectangle(472 + AppLocation.X, 732 + AppLocation.Y, 90, 49);

            Women = new Point(240 + AppLocation.X, 615 + AppLocation.Y);
            Prince = new Point(240 + AppLocation.X, 460 + AppLocation.Y);
            Lady = new Point(240 + AppLocation.X, 310 + AppLocation.Y);

            Immortar = new Point(413 + AppLocation.X, 460 + AppLocation.Y);
            Hunter = new Point(580 + AppLocation.X, 460 + AppLocation.Y);
            Target = new Point(1280 + AppLocation.X, 300 + AppLocation.Y);

            //go to start screen
            ComeBackButton = new Point(50 + AppLocation.X, 80 + AppLocation.Y);

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoGold(int nFood)
        {
            nTimes = nFood / 6;
            for (int i = 0; i < nTimes; i++)
            {
                Opening();

                //main.Log("AutoGold. Detect Dark...");
                //for (int j = 0; j < 2000; j++)
                //    if (DetectSaveTheMonk(100))
                //        break;

                //main.Log("AutoGold. Detect Bright...");
                //for (int j = 0; j < 100; j++)
                //    if (!DetectSaveTheMonk(100))
                //        break;

                //AttackTheMonk(50, 19000);


                Closing(2000);
                main.Log("AutoGold. Food spent: " + (6 * (i + 1)).ToString());

                if (i >= nLoopToSave && (i % nLoopToSave == 0))
                    com.SwitchTab();
            }
            com.SwitchTab();
            //Comeback to start screen

            main.Log("AutoGold. Switch to autoJade");
            com.Restart();
        }

        #region Sequence
        private void Opening()
        {
            if (main.IsStopped) return;
            main.Log("Opening. Begin");
            try
            {
                //main.Log("Opening. Click GatePos");
                for (int i = 0; i < 20; i++)
                    WinAPI.LeftClick(GatePos, 400);
                //main.Log("Opening. Click StartButton");
                WinAPI.LeftClick(StartButton, 5000);
                //main.Log("Opening. Click Speed");
                WinAPI.LeftClick(Speed, 21000);
                //WinAPI.LeftClick(Speed, 62000);

                for (int i = 0; i < 20; i++)
                {
                    Bitmap ClickImage = WinAPI.GetRectImage(ClickRect, false);
                    //pi.Save(ClickImage, "IsFinishBattle");
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

        private bool DetectSaveTheMonk(int sleep)
        {
            //main.Log("DetectSaveTheMonk. Begin");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                //main.Log("WinAPI.GetRectImage");
                Bitmap skyImage = WinAPI.GetRectImage(Sky, false);

                Color leftTop = skyImage.GetPixel(0, 0);
                Color rightTop = skyImage.GetPixel(49, 0);
                Color rightBot = skyImage.GetPixel(49, 49);
                Color leftBot = skyImage.GetPixel(0, 49);
                Color center = skyImage.GetPixel(25, 25);


                int avg = (leftTop.R + rightTop.R + rightBot.R + leftBot.R + center.R) / 5;
                sw.Stop();
                skyImage.Dispose();

                if (avg < 100)
                {

                    //main.Log("DetectSaveTheMonk: dark, " + sw.ElapsedMilliseconds.ToString() + ", avg:" + avg.ToString());
                    Thread.Sleep(sleep);
                    return true;
                }
                //main.Log("DetectSaveTheMonk: bright, " + sw.ElapsedMilliseconds.ToString() + ", avg:" + avg.ToString());
                Thread.Sleep(sleep);
                return false;
            }
            catch (Exception ex)
            {
                main.Log("DetectSaveTheMonk. Ex: " + ex.Message.ToString());
                return true;
            }
        }

        private void AttackTheMonk(int sleep, int timeAttact)
        {
            main.Log("AttackTheMonk. Begin");
            try
            {
                //main.Log("AttackTheMonk. Hunter");
                WinAPI.DragAndDrop(Hunter, Target, 50);
                //main.Log("AttackTheMonk. Women");
                WinAPI.DragAndDrop(Women, Target, 50);
                //main.Log("AttackTheMonk. Prince");
                WinAPI.DragAndDrop(Prince, Target, 50);
                //main.Log("AttackTheMonk. Lady");
                WinAPI.DragAndDrop(Lady, Target, 50);
                //main.Log("AttackTheMonk. Immortar");
                WinAPI.DragAndDrop(Immortar, Target, 50);
                Thread.Sleep(timeAttact);
            }
            catch (Exception ex)
            {
                main.Log("AttackTheMonk. Ex: " + ex.Message.ToString());
            }
            //main.Log("AttackTheMonk. End");
        }

        private void Closing(int sleep)
        {
            if (main.IsStopped) return;
            main.Log("Closing. Begin");
            try
            {
                for (int i = 0; i < 10; i++)
                    WinAPI.LeftClick(GatePos, 200);
                //Thread.Sleep(sleep);
            }
            catch (Exception ex)
            {
                main.Log("Closing. Ex: " + ex.Message.ToString());
            }
            //main.Log("Closing. End");
        }


        #endregion
        //Closing
    }
}
