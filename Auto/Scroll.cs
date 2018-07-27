using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto
{
    public class Scroll
    {
        private Point AppLocation, StartButton, HeroButton, GreatSagePos,DragonPrincePos, LeftArrow, RightArrow, ViewButton, ResetButton;
        private Rectangle ScrollTitle;
        private bool bSuccess;
        private Point XButton1, XButton2, XButton3, ComeBackButton;

        ProcessImage pi;
        private const int nMaxServerErrorTimes = 10, maxLoop = 10000;

        Auto main;
        Common com;
        public Scroll(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;

            //Opening
            StartButton = new Point(700 + AppLocation.X, 750 + AppLocation.Y);
            HeroButton = new Point(50 + AppLocation.X, 790 + AppLocation.Y);
            GreatSagePos = new Point(250 + AppLocation.X, 500 + AppLocation.Y);
            DragonPrincePos = new Point(430+ AppLocation.X, 311 + AppLocation.Y);
            LeftArrow = new Point(50 + AppLocation.X, 440 + AppLocation.Y);
            RightArrow = new Point(1360 + AppLocation.X, 440 + AppLocation.Y);
            ViewButton = new Point(550 + AppLocation.X, 500 + AppLocation.Y);

            //DoScroll
            ResetButton = new Point(450 + AppLocation.X, 200 + AppLocation.Y);
            ScrollTitle = new Rectangle(400 + AppLocation.X, 299 + AppLocation.Y, 127, 38);
            bSuccess = false;

            //Closing and backup
            XButton1 = new Point(1070 + AppLocation.X, 135 + AppLocation.Y);
            XButton2 = new Point(1190 + AppLocation.X, 140 + AppLocation.Y);
            XButton3 = new Point(1340 + AppLocation.X, 110 + AppLocation.Y);
            ComeBackButton = new Point(50 + AppLocation.X, 80 + AppLocation.Y);

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }
        public void AutoScroll(string sText, bool isAutoGold)
        {
            DateTime initialTime = DateTime.Now;
            string[] code = sText.Split(',');

            for (int i = 0; i < maxLoop; i++)
            {
                if (main.IsStopped) break;
                Opening(int.Parse(code[0]));
                bSuccess = DoScroll(int.Parse(code[1]), code[2]);
                if (bSuccess)
                {
                    com.SwitchTab();
                    ClosingAndBackup(isAutoGold, true);
                    com.PlaySound();
                    com.SendMessage("AutoScroll. Success scroll " + code[2] + "!");
                    break;
                }
                else
                {
                    ClosingAndBackup();
                }
            }
            main.SaveLog(initialTime.ToString("[dd_MM]H_mm_ss.fff") + "_Scroll.txt");
        }


        #region Sequence
        private void Opening(int nTimes)
        {
            if (main.IsStopped) return;
            main.Log("Opening. Begin");
            try
            {
                //main.Log("Opening. Click StartButton");
                WinAPI.LeftClick(StartButton, 4000);
                //main.Log("Opening. Click RewardButton");
                for (int i = 0; i < 10; i++)
                    WinAPI.LeftClick(HeroButton, 400);
                WinAPI.LeftClick(DragonPrincePos, 2000);
                for (int i = 0; i < Math.Abs(nTimes) - 1; i++)
                {
                    if (nTimes < 0)
                        WinAPI.LeftClick(LeftArrow, 300);
                    else
                        WinAPI.LeftClick(RightArrow, 300);
                }
                if (nTimes < 0)
                    WinAPI.LeftClick(LeftArrow, 800);
                else if (nTimes>0)
                    WinAPI.LeftClick(RightArrow, 800);
                WinAPI.LeftClick(ViewButton, 3500);
            }
            catch (Exception ex)
            {
                main.Log("Opening. Ex: " + ex.Message.ToString());
                throw;
            }
            //main.Log("Opening. End");
        }

        private bool DoScroll(int nTimes, string title)
        {
            if (main.IsStopped) return false;
            main.Log("DoScroll. Begin");
            try
            {
                for (int i = 0; i < nTimes; i++)
                {
                    //main.Log("Scroll. Click ResetButton");
                    WinAPI.LeftClick(ResetButton, 500);
                    //main.Log("Scroll. Click RewardButton");

                    //main.Log("WinAPI.GetRectImage");
                    Bitmap TitleImage = WinAPI.GetRectImage(ScrollTitle, false);
                    if (pi.CheckStringFromImage(TitleImage, title, 10))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                main.Log("DoScroll. Ex: " + ex.Message.ToString());
                throw;
            }
            //main.Log("Opening. End");
            return false;
        }


        private void ClosingAndBackup(bool isAutoGold = false, bool isAutoJade = false)
        {
            main.Log("ClosingAndBackup. Begin");
            try
            {
                //main.Log("StopAndBackup. Click XButton");
                WinAPI.LeftClick(XButton1, 500);
                WinAPI.LeftClick(XButton2, 500);
                WinAPI.LeftClick(XButton3, 2000);
                //main.Log("StopAndBackup. Click ComeBackButton");
                if (isAutoGold)
                    return; //return to the auto gold screen
                WinAPI.LeftClick(ComeBackButton, 3000);
                if (isAutoJade)
                    return; //return to the main screen

                com.Backup();
            }
            catch (Exception ex)
            {
                main.Log("ClosingAndBackup. Ex: " + ex.Message.ToString());
            }
            //main.Log("StopAndBackup. End");
        }

        #endregion

    }
}
