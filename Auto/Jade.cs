using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Auto
{
    public class Jade
    {
        private bool bSkip = false;
        //Opening
        private Point AppLocation, StartButton, RewardButton, StartMonkeyButton, ConfirmMonkeyButton, StopMonkeyButton;
        private Rectangle JadeRectangle, JadeRectAfter, MonkeyRect;

        private Point XButton, ComeBackButton;

        private int nRestartTime, nRunningTime, nInitialJade;
        private const int nMaxErrorTimes = 2, nMaxRunningTime = 20, maxLoop = 10000, warning = 1000;

        ProcessImage pi;
        DateTime initialTime;

        Auto main;
        Common com;
        public Jade(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;

            nRestartTime = 0;
            nRunningTime = 0;

            //Opening
            StartButton = new Point(700 + AppLocation.X, 750 + AppLocation.Y);
            RewardButton = new Point(1325 + AppLocation.X, 184 + AppLocation.Y);

            //shooting
            StartMonkeyButton = new Point(700 + AppLocation.X, 800 + AppLocation.Y);
            ConfirmMonkeyButton = new Point(710 + AppLocation.X, 630 + AppLocation.Y);
            StopMonkeyButton = new Point(700 + AppLocation.X, 800 + AppLocation.Y);
            JadeRectangle = new Rectangle(340 + AppLocation.X, 73 + AppLocation.Y, 100, 33);
            JadeRectAfter = new Rectangle(1045 + AppLocation.X, 72 + AppLocation.Y, 70, 35);
            MonkeyRect = new Rectangle(1331 + AppLocation.X, 743 + AppLocation.Y, 33, 38);

            //Stop and backup
            XButton = new Point(1350 + AppLocation.X, 110 + AppLocation.Y);
            ComeBackButton = new Point(50 + AppLocation.X, 80 + AppLocation.Y);

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoJade(int nWarning)
        {
            initialTime = DateTime.Now;
            bool bInitSuccess = false;
            int nSpentJade = 0, nLastSavedJade = 0;
            main.ClearLog();
            main.Log("AutoJade. Start time: " + initialTime.ToString("[dd_MM]H_mm_ss_fff"));
            for (int i = 0; i < maxLoop; i++)
            {
                if (main.IsStopped) break;
                main.Log("AutoJade. Loop number: " + i.ToString());
                if (!bSkip)
                    Opening();

                //Get jade before start monkey canon
                //main.Log("WinAPI.GetRectImage");
                Bitmap JadeImageBefore = WinAPI.GetRectImage(JadeRectangle);
                if (JadeImageBefore == null)
                {
                    bSkip = true;
                    continue;
                }

                int JadeBefore = pi.GetNumberFromImage(JadeImageBefore);
                if (!bInitSuccess)
                {
                    if (JadeBefore > 9)
                    {
                        nInitialJade = JadeBefore;
                        nLastSavedJade = JadeBefore;
                        main.Log("AutoJade. Initial Jade: " + nInitialJade.ToString());
                        bInitSuccess = true;
                    }
                }
                else
                {
                    if (JadeBefore != -1)
                    {
                        if (JadeBefore < nLastSavedJade)
                        {
                            nSpentJade += nLastSavedJade - JadeBefore;
                            nLastSavedJade = JadeBefore;
                        }
                        System.TimeSpan timeDiff = DateTime.Now.Subtract(initialTime);
                        int jadeDiff = JadeBefore - nInitialJade + nSpentJade;
                        if (timeDiff.TotalMinutes != 0)
                        {
                            double gain = (double)jadeDiff / timeDiff.TotalMinutes;
                            main.Log("AutoJade. Init: " + nInitialJade.ToString() + ", Spent: " + nSpentJade.ToString() + ", Gain Speed: " + gain.ToString("0.00"));
                        }
                        if (JadeBefore < nInitialJade)
                        {
                            //main.Log("AutoJade. Something was wrong. Break the loop!");
                            // break;
                        }
                    }
                }
                if (JadeBefore == -1)
                {
                    bSkip = true;
                    nRestartTime++;
                    main.Log("AutoJade. Restart Time: " + nRestartTime.ToString());
                    i--;
                    Thread.Sleep(100);
                    if (nRestartTime > nMaxErrorTimes)
                    {
                        nRestartTime = 0;
                        bSkip = false;
                        com.Restart();
                    }
                    continue;
                }

                Shooting();

                //Get jade after start monkey canon
                //main.Log("WinAPI.GetRectImage");
                Bitmap JadeImageAfter = WinAPI.GetRectImage(JadeRectAfter);
                if (JadeImageAfter == null)
                {
                    bSkip = true;
                    continue;
                }

                int JadeAfter = pi.GetNumberFromImage(JadeImageAfter);

                //Check banned
                if (JadeAfter >= nWarning)
                {
                    //if (JadeAfter < banned)
                    //{
                    //    com.SwitchTab();
                    //    WinAPI.LeftClick(ComeBackButton, 2500); //go back to start screen
                    //}
                    //else
                    //StopAndBackup();

                    com.PlaySound();
                    nLastSavedJade = JadeAfter;
                    break;
                }

                if (JadeAfter == -1)
                {
                    bSkip = true;
                    nRestartTime++;
                    main.Log("AutoJade. Restart Time: " + nRestartTime.ToString());
                    i--;
                    Thread.Sleep(100);
                    if (nRestartTime > nMaxErrorTimes)
                    {
                        nRestartTime = 0;
                        bSkip = false;
                        com.Restart();
                    }
                    continue;
                }

                if (JadeAfter > JadeBefore)
                {
                    if (main.cbAudio.Checked)
                        com.PlaySound(1); //noti

                    com.SwitchTab();
                    nLastSavedJade = JadeAfter;

                    bSkip = true;
                    //go back to monkey cannon
                    for (int j = 0; j < 20; j++)
                    {
                        WinAPI.LeftClick(RewardButton, 500);
                        Bitmap MonkeyImage = WinAPI.GetRectImage(MonkeyRect);
                        bool bString = pi.CheckStringFromImage(MonkeyImage, "7", 200, false);
                        if (bString)
                            break;
                    }
                    continue;
                }
                else if (JadeAfter < JadeBefore)
                {
                    bSkip = false;
                    StopAndBackup();
                }
                else
                {
                    bSkip = true;
                    continue;
                }

                nRunningTime++;
                if (nRunningTime > nMaxRunningTime)
                {
                    //Restart();
                    nRunningTime = 0;
                }
            }
            main.Log("AutoJade. End time: " + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff"));
            double totalMinutes = DateTime.Now.Subtract(initialTime).TotalMinutes;
            int totalJade = (nLastSavedJade + nSpentJade - nInitialJade);
            double gainSpeed = (double)totalJade / totalMinutes;
            if (totalMinutes > 10)
                com.SendMessage("AutoJade. Finish. Current Jade: " + nLastSavedJade.ToString());
            main.Log("AutoJade\nTotal jade:\n" + totalJade.ToString() + "\nTotal minutes:\n" + totalMinutes.ToString("0.00") +
                "\nGain speed:\n" + gainSpeed.ToString("0.00"));
            main.SaveLog(initialTime.ToString("[dd_MM]H_mm_ss.fff") + "_Jade.txt");
            main.Log("AutoJade. Saved log file. ");
        }


        #region Sequence

        private void Opening()
        {
            if (main.IsStopped) return;

            main.Log("Opening. Begin");
            try
            {
                //main.Log("Opening. Click StartButton");
                WinAPI.LeftClick(StartButton, 2500);
                for (int i = 0; i < 20; i++)
                {
                    WinAPI.LeftClick(RewardButton, 500);
                    Bitmap MonkeyImage = WinAPI.GetRectImage(MonkeyRect);
                    bool bString = pi.CheckStringFromImage(MonkeyImage, "7", 100, false);
                    if (bString)
                        break;
                }

                // WinAPI.LeftClick(RewardButton, 4000);

            }
            catch (Exception ex)
            {
                main.Log("Opening. Ex: " + ex.Message.ToString());
                throw;
            }
            //main.Log("Opening. End");
        }

        private void Shooting()
        {
            if (main.IsStopped) return;
            main.Log("Shooting. Begin");
            try
            {
                //main.Log("Shooting. Click StartMonkeyButton");
                WinAPI.LeftClick(StartMonkeyButton, 1000);
                //main.Log("Shooting. Click ConfirmMonkeyButton");
                WinAPI.LeftClick(ConfirmMonkeyButton, 710);
                //main.Log("Shooting. Click StopMonkeyButton");
                WinAPI.LeftClick(StopMonkeyButton, 1000);
                //main.Log("Shooting. Click XButton");
                WinAPI.LeftClick(XButton, 2000);
            }
            catch (Exception ex)
            {
                main.Log("Shooting. Ex: " + ex.Message.ToString());
            }
            //main.Log("Shooting. End");
        }

        private void StopAndBackup()
        {
            if (main.IsStopped) return;
            main.Log("StopAndBackup. Begin");
            try
            {
                //main.Log("StopAndBackup. Click XButton");
                //WinAPI.LeftClick(XButton, 2500);
                //main.Log("StopAndBackup. Click ComeBackButton");
                WinAPI.LeftClick(ComeBackButton, 2500);
                com.Backup();
            }
            catch (Exception ex)
            {
                main.Log("StopAndBackup. Ex: " + ex.Message.ToString());
            }
            //main.Log("StopAndBackup. End");
        }

        #endregion

    }
}
