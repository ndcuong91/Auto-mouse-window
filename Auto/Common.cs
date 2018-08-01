using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Auto
{
    //Class has basic functions like save, restart...
    public class Common
    {
        private Point AppLocation, HomeTab, GameTab, CloseTab, GameIcon, OKButton, Messenger;
        private Rectangle OKRect;
        //Backup
        private Point ServerButton, ConfirmButton, ConfirmAgainButton;
        private Rectangle JadeBoundingBox, StartBoundingBox, BackupRect;
        private const int nMaxServerErrorTimes = 10;
        string finish, noti;

        Auto main;
        ProcessImage pi;
        public Common(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;
            //switch tab
            HomeTab = new Point(244 + AppLocation.X, 24 + AppLocation.Y);
            GameTab = new Point(444 + AppLocation.X, 24 + AppLocation.Y);

            //restart
            GameIcon = new Point(244 + AppLocation.X, 182 + AppLocation.Y);
            CloseTab = new Point(525 + AppLocation.X, 14 + AppLocation.Y);

            //Backup
            ServerButton = new Point(1355 + AppLocation.X, 453 + AppLocation.Y);
            ConfirmButton = new Point(850 + AppLocation.X, 630 + AppLocation.Y);
            ConfirmAgainButton = new Point(700 + AppLocation.X, 630 + AppLocation.Y);
            JadeBoundingBox = new Rectangle(430 + AppLocation.X, 405 + AppLocation.Y, 85, 40);
            StartBoundingBox = new Rectangle(614 + AppLocation.X, 726 + AppLocation.Y, 188, 50);

            //TitleBar
            OKRect = new Rectangle(1125 + AppLocation.X, 571 + AppLocation.Y, 36, 16);
            OKButton = new Point(1143 + AppLocation.X, 579 + AppLocation.Y);
            Messenger = new Point(380, 958);

            BackupRect = new Rectangle(405 + AppLocation.X, 891 + AppLocation.Y, 76, 23);

            finish = @"D:\8. Games\SecretKingdom\finish.wav";
            noti = @"D:\8. Games\SecretKingdom\noti.wav";
            main = mainForm;
        }

        /// <summary>
        /// Save current state
        /// </summary>
        public void SwitchTab()
        {
            if (main.IsStopped) return;
            main.Log("SwitchTab. Begin");
            try
            {
                //main.Log("SwitchTab. Click HomeTab");
                WinAPI.LeftClick(HomeTab, 1000);
                //main.Log("SwitchTab. Click GameTab");
                WinAPI.LeftClick(GameTab, 6000);

            }
            catch (Exception ex)
            {
                main.Log("SwitchTab. Ex: " + ex.Message.ToString());
            }
            //main.Log("SwitchTab. End");
        }

        public void Backup()
        {
            if (main.IsStopped) return;
            //main.Log("Backup. Begin");
            try
            {
                WinAPI.LeftClick(ServerButton, 2000);

                bool bCheckString = false;
                for (int i = 0; i < nMaxServerErrorTimes; i++)
                {
                    //main.Log("WinAPI.GetRectImage");
                    Bitmap JadeImage = WinAPI.GetRectImage(JadeBoundingBox);
                    bCheckString = pi.CheckStringFromImage(JadeImage, "jade", 500, false);
                    if (bCheckString)
                    {
                        //main.Log("StopAndBackup. Click ConfirmButton");
                        WinAPI.LeftClick(ConfirmButton, 1000);
                        //main.Log("StopAndBackup. Click ConfirmAgainButton");
                        WinAPI.LeftClick(ConfirmAgainButton, 1500);
                        //main.Log("StopAndBackup. Click GameIcon");
                        WinAPI.LeftClick(GameIcon, 7000);

                        for (int n = 0; n < 100; n++)
                        {

                            Bitmap StartImage = WinAPI.GetRectImage(StartBoundingBox);
                            bool bString = pi.CheckStringFromImage(StartImage, "start", 200, false, true);
                            if (bString)
                                return;
                        }

                        break;
                    }
                }
                if (!bCheckString)
                {
                    Restart();
                }

            }
            catch (Exception ex)
            {
                main.Log("Backup. Ex: " + ex.Message.ToString());
            }
            //main.Log("Backup. End");
        }

        public void Test()
        {

            Bitmap StartImage = WinAPI.GetRectImage(StartBoundingBox);
            bool bCheckString = pi.CheckStringFromImage(StartImage, "start", 1000, true, true);
        }

        public void Restart()
        {
            if (main.IsStopped) return;
            main.Log("Restart. Begin");
            try
            {
                //main.Log("Restart. Click CloseTab");
                WinAPI.LeftClick(CloseTab, 2000);
                //main.Log("Restart. Click GameTab");
                WinAPI.LeftClick(GameIcon, 7000);


                for (int n = 0; n < 100; n++)
                {
                    Bitmap StartImage = WinAPI.GetRectImage(StartBoundingBox);
                    bool bString = pi.CheckStringFromImage(StartImage, "start", 200, false, true);
                    if (bString)
                        return;
                }
            }
            catch (Exception ex)
            {
                main.Log("Restart. Ex: " + ex.Message.ToString());
            }
            //main.Log("Restart. End");
        }

        public bool CheckButton(Point button)
        {
            Rectangle teamview = new Rectangle(707, 428, 504, 183);
            if (teamview.Contains(button))
                return true;
            return false;
        }

        public void PlaySound(int type = 0)
        {
            main.Log("PlaySound. Begin");
            string soundFile;
            if (type == 0)//finish
                soundFile = finish;
            else
                soundFile = noti;
            using (var player = new System.Media.SoundPlayer(soundFile))
                player.Play();
        }

        public void CheckTeamviewer()
        {
            Bitmap OKImage = WinAPI.GetRectImage(OKRect);
            bool bCheckString = pi.CheckStringFromImage(OKImage, "ok", 100, false);
            if (bCheckString)
            {
                main.Log("CheckTeamviewer. True");
                WinAPI.LeftClick(OKButton, 200);
            }
            //else
                //main.Log("CheckTeamviewer. False");
        }

        public void SendMessage(string message)
        {
            for (int i = 0; i < 5; i++)
                WinAPI.LeftClick(Messenger, 100);

            foreach (char ch in message)
                SendKeys.SendWait(ch.ToString());
            SendKeys.SendWait("{ENTER}");
        }

        public bool CheckBackupMessage()
        {
            if (main.IsStopped) return false;
            main.Log("CheckBackupMessage. Begin");
            try
            {
                Bitmap BackupImage = WinAPI.GetRectImage(BackupRect);
                bool bBackup = pi.CheckStringFromImage(BackupImage, "backup", 1000, false, false, true);
                if (bBackup)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                main.Log("CheckBackupMessage. Ex: " + ex.Message.ToString());
            }
            return false;
        }

    }
}
