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
    public class SafeJade_v2
    {
        private Point AppLocation;
        private int nTimes;
        private const int nLoopToSave = 5;

        //Opening
        private Point GatePos, StartButton, Speed, StopButton, RestartButton;
        private Rectangle Battle, HeroesRect, Sky;

        //Attack The Monk
        private Point Hunter, Immortar, Prince, Women, Lady, Target;

        //switch tab
        private Point ComeBackButton, JadePosition;
        private const int maxLoop = 10000;

        ProcessImage pi;
        Common com;
        Auto main;
        public SafeJade_v2(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            pi.InitSearch();
            AppLocation = appLocation;
            //Opening
            GatePos = new Point(709 + AppLocation.X, 726 + AppLocation.Y);
            StartButton = new Point(1290 + AppLocation.X, 764 + AppLocation.Y);
            Speed = new Point(1042 + AppLocation.X, 95 + AppLocation.Y);
            HeroesRect = new Rectangle(225 + AppLocation.X, 125 + AppLocation.Y, 135, 46);

            //Battle image
            Battle = new Rectangle(117 + AppLocation.X, 196 + AppLocation.Y, 1103, 514);
            Women = new Point(240 + AppLocation.X, 615 + AppLocation.Y);
            Prince = new Point(240 + AppLocation.X, 460 + AppLocation.Y);
            Lady = new Point(240 + AppLocation.X, 310 + AppLocation.Y);
            Immortar = new Point(413 + AppLocation.X, 460 + AppLocation.Y);
            Hunter = new Point(580 + AppLocation.X, 460 + AppLocation.Y);
            Target = new Point(1280 + AppLocation.X, 300 + AppLocation.Y);
            Sky = new Rectangle(170 + AppLocation.X, 720 + AppLocation.Y, 50, 50);

            //Restart Stage
            StopButton = new Point(1166 + AppLocation.X, 91 + AppLocation.Y);
            RestartButton = new Point(1057 + AppLocation.X, 442 + AppLocation.Y);

            //go to start screen
            ComeBackButton = new Point(50 + AppLocation.X, 80 + AppLocation.Y);
            nTimes = 0;

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoSafeJade(int nJade)
        {
            //com.SendMessage("Test");
            DateTime initialTime = DateTime.Now;
            main.ClearLog();
            main.Log("AutoSafeJade. Need: " + nJade.ToString() + " Jade");
            main.Log("AutoSafeJade. Start time: " + initialTime.ToString("[dd_MM]H_mm_ss.fff"));
            nTimes = nJade / 5;
            int nSuccessTimes = 0;
            for (int i = 0; i < maxLoop; i++)
            {
                if (main.IsStopped) break;
                main.Log("AutoSafeJade. Loop number: " + i.ToString() + ", Loop found jade: " + nSuccessTimes.ToString());
                if (i==0)
                    Opening(true);
                else
                    Opening(false);

                main.Log("AutoSafeJade. Detect Jade or Pearl...");
                for (int j = 0; j < 300; j++)
                {
                    if (main.IsStopped) break;
                    int stt = DetectGift(100);
                    //main.Log("AutoSafeJade. Status: " + stt.ToString());
                    if (stt == 1)  //jade
                    {
                        Thread.Sleep(1000);//wait for jade stop moving
                        DetectGift(100); //get correct jade position

                        if (main.cbAudio.Checked)
                            com.PlaySound(1); //noti

                        com.CheckTeamviewer();
                        WinAPI.LeftClick(Hunter, 500);
                        main.Log("AutoSafeJade. Pick up jade in position: " + JadePosition.X.ToString() + ", " + JadePosition.Y.ToString());
                        WinAPI.LeftClick(JadePosition, 500);

                        nSuccessTimes++;
                        if (nSuccessTimes % 4 == 0)
                            com.SendMessage("AutoSafeJade. Collect: " + (5 * nSuccessTimes).ToString() + " Jade!");

                        main.Log("AutoSafeJade. Detect Dark Screen...");
                        for (int k = 0; k < 1000; k++)
                            if (DetectSaveTheMonk(100))
                                break;

                        main.Log("AutoSafeJade. Detect Normal Screen...");
                        for (int k = 0; k < 100; k++)
                            if (!DetectSaveTheMonk(100))
                                break;

                        AttackTheMonk(50, 15000);

                        break;
                    }

                    if (stt == 0)  //pearl
                    {
                        //com.SendMessage("AutoSafeJade. Found Pearl!");
                        RestartStage();
                        break;
                    }
                }

                double minutes = DateTime.Now.Subtract(initialTime).TotalMinutes;
                double gain = (double)(5 * nSuccessTimes) / minutes;
                main.Log("AutoSafeJade. Jade earned: " + (5 * nSuccessTimes).ToString() + ", Gain Speed: " + gain.ToString("0.00"));
                if (nSuccessTimes >= nTimes)
                {
                    com.PlaySound();
                    Thread.Sleep(15000);
                    main.Log("AutoSafeJade. End. Click Gate.");
                    WinAPI.LeftClick(GatePos, 100);
                    break;
                }
            }
            main.Log("AutoSafeJade. End time: " + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff"));

            double totalMinutes = DateTime.Now.Subtract(initialTime).TotalMinutes;
            double gainSpeed = (double)(5 * nSuccessTimes) / totalMinutes;
            main.Log("AutoSafeJade\nTotal jade:\n" + (5 * nSuccessTimes).ToString() + "\nTotal minutes:\n" + totalMinutes.ToString("0.00") +
                "\nGain speed:\n" + gainSpeed.ToString("0.00"));
            if (totalMinutes > 10)
                com.SendMessage("AutoSafeJade. Finish collect: " + (5 * nSuccessTimes).ToString() + " Jade!");
            main.SaveLog(initialTime.ToString("[dd_MM]H_mm_ss.fff") + "_SafeJade.txt");
            main.Log("AutoSafeJade. Saved log file. ");
            //Comeback to start screen
        }

        #region Sequence
        private void Opening(bool bFirstTime=false)
        {
            if (main.IsStopped) return;
            main.Log("Opening. Begin");
            try
            {
                com.CheckTeamviewer();
                bool bSaveImage = false;
                for (int i = 0; i < 20; i++)
                {
                    //main.Log("loop: " + i.ToString());
                    if (i > 3)
                        bSaveImage = true;

                    WinAPI.LeftClick(GatePos, 500);
                    Bitmap HeroesImage = WinAPI.GetRectImage(HeroesRect);
                    bool bString = pi.CheckStringFromImage(HeroesImage, "hero", 100, false, true, bSaveImage);
                    if (bString)
                        break;
                }
                Thread.Sleep(2000);
                WinAPI.LeftClick(StartButton, 5000);
                WinAPI.LeftClick(Speed, 1000);
            }
            catch (Exception ex)
            {
                main.Log("Opening. Ex: " + ex.ToString());
            }
            //main.Log("Opening. End");
        }

        private int DetectGift(int sleep)
        {
            if (main.IsStopped) return -1;
            //main.Log("DetectGift. Begin");
            try
            {
                com.CheckTeamviewer();
                Bitmap BattleImage = WinAPI.GetRectImage(Battle, false);
                int nStatus = pi.FindJade(BattleImage);
                BattleImage.Dispose();
                if (nStatus == 1)
                {
                    main.Log("DetectGift. Jade. Score: " + pi.GetScore());
                    Point raw = pi.GetJadePosition();
                    JadePosition.X = raw.X + Battle.Left;
                    JadePosition.Y = raw.Y + Battle.Top;
                }
                if (nStatus == 0)
                {
                    main.Log("DetectGift. Pearl. Score: " + pi.GetScore());
                }
                Thread.Sleep(sleep);
                return nStatus;
            }
            catch (Exception ex)
            {
                main.Log("DetectGift. Ex: " + ex.ToString());
                Thread.Sleep(sleep);
                return -1;
            }
        }

        private bool DetectSaveTheMonk(int sleep)
        {
            //main.Log("DetectSaveTheMonk. Begin");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
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
                    Thread.Sleep(sleep);
                    return true;
                }
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
                com.CheckTeamviewer();
                WinAPI.DragAndDrop(Hunter, Target, 50);
                WinAPI.DragAndDrop(Women, Target, 50);
                WinAPI.DragAndDrop(Prince, Target, 50);
                WinAPI.DragAndDrop(Lady, Target, 50);
                WinAPI.DragAndDrop(Immortar, Target, 50);
                Thread.Sleep(timeAttact);
            }
            catch (Exception ex)
            {
                main.Log("AttackTheMonk. Ex: " + ex.ToString());
            }
            // main.Log("AttackTheMonk. End");
        }

        private void RestartStage()
        {
            main.Log("RestartStage. Begin");
            try
            {
                com.CheckTeamviewer();
                WinAPI.LeftClick(StopButton, 500);
                WinAPI.LeftClick(RestartButton, 500);
            }
            catch (Exception ex)
            {
                main.Log("RestartStage. Ex: " + ex.ToString());
            }
            //main.Log("RestartStage. End");
        }

        #endregion
    }

}
