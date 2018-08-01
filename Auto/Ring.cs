using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace Auto
{
    public class Ring
    {
        private Point AppLocation, StartButton, TaskbarButton, PVPButton;
        private Point RefreshButton, SpeedButton, Screen, XButton, ComeBackButton;
        private Point[] FightButton;
        private Rectangle[] RankRect, TeamPowerRect, Gift;
        private Rectangle ClickRect, WinOrLooseRect;
        private bool bSuccess, bSkip;
        private Point BuyMoreButton, ConfirmButton;

        int currentRank, nBattle;
        private const int nMaxServerErrorTimes = 10, maxLoop = 10000, maxRefresh = 20;

        ProcessImage pi;
        Auto main;
        Common com;
        public Ring(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;
            FightButton = new Point[3];
            RankRect = new Rectangle[3];
            TeamPowerRect = new Rectangle[3];
            Gift = new Rectangle[3];
            //Opening
            StartButton = new Point(700 + AppLocation.X, 750 + AppLocation.Y);
            TaskbarButton = new Point(1344 + AppLocation.X, 770 + AppLocation.Y);
            PVPButton = new Point(880 + AppLocation.X, 770 + AppLocation.Y);

            //Battle
            int yBase = 126, yStart = 345;
            RefreshButton = new Point(1190 + AppLocation.X, 240 + AppLocation.Y);
            SpeedButton = new Point(1335 + AppLocation.X, 90 + AppLocation.Y);
            Screen = new Point(710 + AppLocation.X, 600 + AppLocation.Y);


            FightButton[0] = new Point(1175 + AppLocation.X, 340 + AppLocation.Y);
            FightButton[1] = new Point(1175 + AppLocation.X, 340 + yBase + AppLocation.Y);
            FightButton[2] = new Point(1175 + AppLocation.X, 340 + 2 * yBase + AppLocation.Y);

            RankRect[0] = new Rectangle(800 + AppLocation.X, yStart + AppLocation.Y, 44, 30);
            RankRect[1] = new Rectangle(800 + AppLocation.X, yStart + yBase + AppLocation.Y, 44, 30);
            RankRect[2] = new Rectangle(800 + AppLocation.X, yStart + 2 * yBase + AppLocation.Y, 44, 30);

            TeamPowerRect[0] = new Rectangle(1033 + AppLocation.X, yStart + AppLocation.Y, 68, 30);
            TeamPowerRect[1] = new Rectangle(1033 + AppLocation.X, yStart + yBase + AppLocation.Y, 68, 30);
            TeamPowerRect[2] = new Rectangle(1033 + AppLocation.X, yStart + 2 * yBase + AppLocation.Y, 68, 30);

            int xBase = 145, xStart = 521;
            Gift[0] = new Rectangle(-5 + xStart + AppLocation.X, 223 + AppLocation.Y, 102, 102);
            Gift[1] = new Rectangle(-5 + xStart + xBase + AppLocation.X, 223 + AppLocation.Y, 102, 102);
            Gift[2] = new Rectangle(-5 + xStart + 2 * xBase + AppLocation.X, 223 + AppLocation.Y, 102, 102);
            //
            ClickRect = new Rectangle(472 + AppLocation.X, 732 + AppLocation.Y, 90, 49);
            WinOrLooseRect = new Rectangle(502 + AppLocation.X, 365 + AppLocation.Y, 50, 50);

            //StopAndBackup
            XButton = new Point(1350 + AppLocation.X, 110 + AppLocation.Y);
            ComeBackButton = new Point(50 + AppLocation.X, 80 + AppLocation.Y);

            //Buy More
            BuyMoreButton = new Point(1228 + AppLocation.X, 703 + AppLocation.Y);
            ConfirmButton = new Point(710 + AppLocation.X, 633 + AppLocation.Y);
            bSkip = false;

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoRing(string sText)
        {
            string[] code = sText.Split(',');
            int nTimeLeft = int.Parse(code[0]);

            for (int i = 0; i < maxLoop; i++)
            {
                main.Log("AutoRing. Loop number: " + i.ToString());
                if (main.IsStopped) break;
                main.Log("AutoRing. times left: " + nTimeLeft.ToString());
                if (!bSkip)
                    Opening();

                nTimeLeft = Battle(nTimeLeft, int.Parse(code[1]), int.Parse(code[2]));
                if (nTimeLeft > 0)
                {
                    bSkip = false;
                    StopAndBackup();
                }
                else
                {
                    BuyMoreBattle();
                    nTimeLeft = 5;
                    com.SwitchTab(); //Save
                    bSkip = true;
                }
            }
        }

        private void Opening()
        {
            if (main.IsStopped) return;
            main.Log("Opening. Begin");
            try
            {
                //main.Log("Opening. Click StartButton");
                WinAPI.LeftClick(StartButton, 3000);
                //main.Log("Opening. Click RewardButton");
                WinAPI.LeftClick(TaskbarButton, 1500);
                for (int i = 0; i < 10; i++)
                    WinAPI.LeftClick(PVPButton, 500);
            }
            catch (Exception ex)
            {
                main.Log("Opening. Ex: " + ex.Message.ToString());
            }
            //main.Log("Opening. End");
        }


        private int Battle(int nTimes, int nMaxRank, int nMaxTeamPower)
        {
            if (main.IsStopped) return -1;
            main.Log("Battle. Begin");
            int nTimesLeft = nTimes;
            int nNewMaxRank = nMaxRank;
            int nRetry = 0;
            int nRestart = 0;
            try
            {
                for (int n = 0; n < nTimes; n++)
                {
                    bool bCheckOpponent = false;
                    for (int i = 0; i < 3; i++)
                    {
                        bCheckOpponent = CheckOpponent(i, nNewMaxRank, nMaxTeamPower);
                        if (bCheckOpponent)
                        {
                            nRetry = 0;
                            nRestart = 0;
                            nNewMaxRank = nMaxRank;
                            main.Log("Battle " + (n + 1).ToString() + ", Detect finished...");
                            bool bIsHasGoldeRing = false;
                            WinAPI.LeftClick(FightButton[i], 7000);
                            WinAPI.LeftClick(SpeedButton, 20000);
                            for (int j = 0; j < 300; j++)
                            {
                                if (IsFinishBattle(100))
                                {
                                    main.Log("Battle. Detect win or lose...");
                                    if (IsWin())
                                    {
                                        main.Log("Battle. Won. Detect gold ring...");
                                        for (int k = 0; k < 3; k++)  //check gift
                                        {
                                            if (IsHasGoldRing(k))
                                            {
                                                main.Log("Yayyyyyyyyyyyyyyyyyyy. There is a gold ring !!!");
                                                bIsHasGoldeRing = true;
                                                com.SwitchTab(); //Save
                                                com.SwitchTab(); //Save
                                                nTimesLeft = nTimes - n - 1;
                                                break;
                                            }
                                        }
                                        if (!bIsHasGoldeRing)
                                            main.Log("No gold ring");
                                    }
                                    else
                                    {
                                        main.Log("Battle. Lose. ");
                                        n--;
                                    }
                                    WinAPI.LeftClick(Screen, 4000);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (!bCheckOpponent)
                    {
                        main.Log("Battle. No opponent satisfied.");
                        n--;
                        WinAPI.LeftClick(RefreshButton, 2000);
                        nRetry++;
                        nRestart++;
                        if (nRetry > 10)
                        {
                            nRetry = 0;
                            nNewMaxRank = nNewMaxRank + 200;
                        }
                        if (nRestart > 25)
                        {
                            com.Restart();
                            Opening();
                            return nTimesLeft;
                        }
                    }
                }

                //if battle left is too small -->buy more 
                if (nTimesLeft == 1)
                {
                    main.Log("Battle. Too small battle left. Buy more.");
                    BuyMoreBattle();
                    com.SwitchTab();
                    nTimesLeft = 5;
                }
            }
            catch (Exception ex)
            {
                main.Log("Battle. Ex: " + ex.Message.ToString());
            }
            //main.Log("Battle. End");
            return nTimesLeft;
        }

        private bool CheckOpponent(int pos, int nMaxRank, int nMaxTeamPower)
        {
            if (main.IsStopped) return false;
            //main.Log("CheckOpponent. Begin");
            try
            {
                Bitmap rank = WinAPI.GetRectImage(RankRect[pos]);
                int nRank = pi.GetNumberFromImage(rank);

                Bitmap power = WinAPI.GetRectImage(TeamPowerRect[pos]);
                int nPower = pi.GetNumberFromImage(power);

                if (nRank > 0 && nPower > 0 && nRank < nMaxRank && nPower < nMaxTeamPower)
                    return true;
            }
            catch (Exception ex)
            {
                main.Log("CheckOpponent. Ex: " + ex.Message.ToString());
                throw;
            }
            //main.Log("Opening. End");
            return false;
        }

        private bool IsFinishBattle(int sleep)
        {
            //main.Log("IsFinishBattle. Begin");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                //main.Log("WinAPI.GetRectImage");
                Bitmap ClickImage = WinAPI.GetRectImage(ClickRect, false);
                //pi.Save(ClickImage, "IsFinishBattle");
                bool bCheckString = pi.CheckStringFromImage(ClickImage, "click", 1000, false);
                if (bCheckString)
                {
                    Thread.Sleep(sleep);
                    return true;
                }

                //DragAndDrop(Lady, Enemy1, 40);
                //DragAndDrop(Hunter, Enemy2, 40);
                //DragAndDrop(Women, Enemy3, 40);
                //DragAndDrop(Immortar, Immortar, 40);

            }
            catch (Exception ex)
            {
                main.Log("IsFinishBattle. Ex: " + ex.Message.ToString());
                return false;
            }
            Thread.Sleep(sleep);
            return false;
        }

        private bool IsWin()
        {
            //main.Log("IsWin. Begin");
            try
            {
                //main.Log("WinAPI.GetRectImage");
                Bitmap WinOrLoose = WinAPI.GetRectImage(WinOrLooseRect, false);
                //pi.Save(WinOrLoose, "WinOrLose");
                Color center = WinOrLoose.GetPixel(WinOrLoose.Width / 2, WinOrLoose.Height / 2);
                WinOrLoose.Dispose();

                if (center.R < 125)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                main.Log("IsWin. Ex: " + ex.Message.ToString());
                return true;
            }
        }

        private bool IsHasGoldRing(int pos)
        {
            //main.Log("IsHasGoldRing. Begin");
            try
            {
                Bitmap GiftImg = WinAPI.GetRectImage(Gift[pos], false);
                Color center = GiftImg.GetPixel(5, 5);
                //pi.Save(GiftImg, "Gift_" + center.G.ToString());
                //main.Log("IsHasGoldRing. Center.B = " + center.B.ToString());
                if (center.G < 110) //jade or gold
                    pi.Save(GiftImg, "D:\\8. Games\\SecretKingdom\\JadeGold\\", center.G.ToString());
                else if (center.G < 135)
                    pi.Save(GiftImg, "D:\\8. Games\\SecretKingdom\\Food\\", center.G.ToString());
                else
                    pi.Save(GiftImg, "D:\\8. Games\\SecretKingdom\\Ring\\", center.G.ToString());
                GiftImg.Dispose();
                if (center.G > 190)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                main.Log("IsHasGoldRing. Ex: " + ex.Message.ToString());
                return false;
            }
        }

        private void StopAndBackup()
        {
            if (main.IsStopped) return;
            main.Log("StopAndBackup. Begin");
            try
            {
                //main.Log("StopAndBackup. Click XButton");
                WinAPI.LeftClick(XButton, 4000);
                //main.Log("StopAndBackup. Click ComeBackButton");
                WinAPI.LeftClick(ComeBackButton, 3000);
                com.Backup();
            }
            catch (Exception ex)
            {
                main.Log("StopAndBackup. Ex: " + ex.Message.ToString());
            }
            //main.Log("StopAndBackup. End");
        }

        private void BuyMoreBattle()
        {
            if (main.IsStopped) return;
            main.Log("BuyMoreBattle. Begin");
            try
            {
                //main.Log("StopAndBackup. Click XButton");
                WinAPI.LeftClick(BuyMoreButton, 3000);
                //main.Log("StopAndBackup. Click ComeBackButton");
                WinAPI.LeftClick(ConfirmButton, 3000);
            }
            catch (Exception ex)
            {
                main.Log("BuyMoreBattle. Ex: " + ex.Message.ToString());
            }
            //main.Log("StopAndBackup. End");
        }
    }
}
