<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auto
{
    public class Gift
    {
        private Point AppLocation, DragFirst, DragSecond, PointToCheck, XButton, GatePos, StartButton, Speed, ConfirmButton, PurchaseButton;
        ProcessImage pi;
        private Rectangle GiftRect, ClickRect, ConfirmRect, DoneRect, JadeRectAfter;
        private const int maxLoop = 10000;

        private Point[] GiftPos;
        int yBase, yIncr, xBase;

        Common com;
        Auto main;
        public Gift(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;


            DragFirst = new Point(200 + AppLocation.X, 800 + AppLocation.Y);
            DragSecond = new Point(650 + AppLocation.X, 800 + AppLocation.Y);
            PointToCheck = new Point(59 + AppLocation.X, 611 + AppLocation.Y);

            GiftRect = new Rectangle(AppLocation.X, AppLocation.Y, 160, 700);
            XButton = new Point(1345 + AppLocation.X, 100 + AppLocation.Y);

            //Opening
            GatePos = new Point(196 + AppLocation.X, 417 + AppLocation.Y);
            //GatePos = new Point(735 + AppLocation.X, 555 + AppLocation.Y);
            StartButton = new Point(1290 + AppLocation.X, 764 + AppLocation.Y);
            Speed = new Point(1227 + AppLocation.X, 95 + AppLocation.Y);

            ClickRect = new Rectangle(472 + AppLocation.X, 732 + AppLocation.Y, 90, 49);

            GiftPos = new Point[2];
            GiftPos[0] = new Point(947 + AppLocation.X, 306 + AppLocation.Y);
            GiftPos[1] = new Point(1125 + AppLocation.X, 306 + AppLocation.Y);
            ConfirmButton = new Point(707 + AppLocation.X, 626 + AppLocation.Y);
            PurchaseButton = new Point(696 + AppLocation.X, 632 + AppLocation.Y);
            JadeRectAfter = new Rectangle(1045 + AppLocation.X, 72 + AppLocation.Y, 70, 35);

            yBase = 360;
            yIncr = 40;
            xBase = 535;
            ConfirmRect = new Rectangle(AppLocation.X + 638, AppLocation.Y + 609, 136, 34);
            DoneRect = new Rectangle(AppLocation.X + 413, AppLocation.Y + 889, 48, 27);

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoGift(int nPos, bool bSave)
        {

            DateTime initialTime = DateTime.Now;
            main.ClearLog();
            main.Log("AutoGift. Start time: " + initialTime.ToString("[dd_MM]H_mm_ss.fff"));
            GotoTheLeft(10);
            Thread.Sleep(2000);
            for (int i = 0; i < 600; i++)
            {
                if (CheckMerchant())
                {
                    if (i == 0)
                        main.Log("AutoGift. Merchant not finish. ");
                    Thread.Sleep(1000);
                }
                else
                    break;
            }
            main.Log("AutoGift. Merchant finished. ");
            Thread.Sleep(2000);

            for (int i = 0; i < maxLoop; i++)
            {
                if (main.IsStopped) return;
                Opening();
                Closing(6000);

                if (CheckMerchant())
                {
                    main.Log("AutoGift. Merchant!!!!! ");
                    com.PlaySound(1);
                    break;
                }
                else
                {
                    main.Log("AutoGift. No Merchant! ");
                }

            }
            bool bSuccess = BuyMerchant(nPos);
            CheckFlagToSave();
            com.PlaySound();
            double totalMinutes = DateTime.Now.Subtract(initialTime).TotalMinutes;
            if (bSave)
            {
                Bitmap JadeImageAfter = WinAPI.GetRectImage(JadeRectAfter);
                int JadeAfter = pi.GetNumberFromImage(JadeImageAfter);
                if (JadeAfter < 1000)
                {
                    com.SwitchTab();
                    if (bSuccess && totalMinutes > 2)
                        com.SendMessage("AutoGift. Saved Done.");
                }
            }
            main.SaveLog(initialTime.ToString("[dd_MM]H_mm_ss.fff") + "_Gift.txt");
            main.Log("AutoGift. Saved log file. ");
        }

        public void GotoTheLeft(int nTime)
        {
            if (main.IsStopped) return;
            main.Log("GotoTheLeft. Begin");
            try
            {
                for (int i = 0; i < nTime; i++)
                    WinAPI.DragAndDrop(DragFirst, DragSecond, 1000);
            }
            catch (Exception ex)
            {
                main.Log("GotoTheLeft. Ex: " + ex.Message.ToString());
            }
        }

        public bool CheckMerchant()
        {
            if (main.IsStopped) return false;
            //main.Log("CheckMerchant. Begin");
            try
            {
                Bitmap GiftImage = WinAPI.GetRectImage(GiftRect, false);
                Color leftTop = GiftImage.GetPixel(PointToCheck.X, PointToCheck.Y);
                //main.Log(string.Format("GotoTheLeft. R:{0}, G:{1}, B:{2} ", leftTop.R, leftTop.G, leftTop.B));
                if (leftTop.B < 100)
                    return true;
            }
            catch (Exception ex)
            {
                main.Log("CheckMerchant. Ex: " + ex.Message.ToString());
            }
            return false;
        }

        public bool BuyMerchant(int nPos) //nPos =1 or 2
        {
            if (main.IsStopped) return false;
            main.Log("BuyMerchant. Begin");
            try
            {
                WinAPI.LeftClick(PointToCheck, 1500);
                WinAPI.LeftClick(PointToCheck, 1500);
                WinAPI.LeftClick(GiftPos[nPos - 1], 1500);
                WinAPI.LeftClick(PurchaseButton, 1500);
                //for (int i = 0; i < 10; i++)
                //{
                //    WinAPI.LeftClick(new Point(xBase, yBase + yIncr * i), 500);
                //    Bitmap ConfirmImage = WinAPI.GetRectImage(ConfirmRect);
                //    bool bString = pi.CheckStringFromImage(ConfirmImage, "confirm", 200, true, true, true, 80);
                //    if (bString)
                //    {
                //        com.SendMessage("AutoGift. Buy Merchant at slot: " + nPos.ToString() + " done!");
                //        break;
                //    }
                //}
                com.SendMessage("AutoGift. Buy Merchant at slot: " + nPos.ToString() + " done! waiting for command to save..........");
                WinAPI.LeftClick(ConfirmButton, 1000);
                WinAPI.LeftClick(XButton, 3000);
                return true;
            }
            catch (Exception ex)
            {
                main.Log("BuyMerchant. Ex: " + ex.Message.ToString());
            }
            return false;
        }

        public void CheckFlagToSave()
        {
            if (main.IsStopped) return;
            main.Log("CheckFlagToSave. Begin");
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    //Bitmap DoneImage = new Bitmap(@"D:\8. Games\SecretKingdom\gift\confirmmerchant.jpg");
                    Bitmap DoneImage = WinAPI.GetRectImage(DoneRect);
                    bool bString = pi.CheckStringFromImage(DoneImage, "done", 1000, false, false, true);
                    if (bString)
                        break;
                }
            }
            catch (Exception ex)
            {
                main.Log("CheckFlagToSave. Ex: " + ex.Message.ToString());
            }
            return;
        }

        private void Opening()
        {
            if (main.IsStopped) return;
            main.Log("Opening. Begin");
            try
            {
                for (int i = 0; i < 20; i++)
                    WinAPI.LeftClick(GatePos, 400);
                WinAPI.LeftClick(StartButton, 5000);
                WinAPI.LeftClick(Speed, 21000);

                for (int i = 0; i < 20; i++)
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

    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auto
{
    public class Gift
    {
        private Point AppLocation, DragFirst, DragSecond, PointToCheck, XButton, GatePos, StartButton, Speed, ConfirmButton, PurchaseButton;
        ProcessImage pi;
        private Rectangle GiftRect, ClickRect, ConfirmRect, DoneRect, JadeRectAfter;
        private const int maxLoop = 10000;

        private Point[] GiftPos;
        int yBase, yIncr, xBase;

        Common com;
        Auto main;
        public Gift(Point appLocation, Auto mainForm)
        {
            pi = new ProcessImage(mainForm);
            AppLocation = appLocation;


            DragFirst = new Point(200 + AppLocation.X, 800 + AppLocation.Y);
            DragSecond = new Point(650 + AppLocation.X, 800 + AppLocation.Y);
            PointToCheck = new Point(59 + AppLocation.X, 611 + AppLocation.Y);

            GiftRect = new Rectangle(AppLocation.X, AppLocation.Y, 160, 700);
            XButton = new Point(1345 + AppLocation.X, 100 + AppLocation.Y);

            //Opening
            GatePos = new Point(196 + AppLocation.X, 417 + AppLocation.Y);
            //GatePos = new Point(735 + AppLocation.X, 555 + AppLocation.Y);
            StartButton = new Point(1290 + AppLocation.X, 764 + AppLocation.Y);
            Speed = new Point(1227 + AppLocation.X, 95 + AppLocation.Y);

            ClickRect = new Rectangle(472 + AppLocation.X, 732 + AppLocation.Y, 90, 49);

            GiftPos = new Point[2];
            GiftPos[0] = new Point(947 + AppLocation.X, 306 + AppLocation.Y);
            GiftPos[1] = new Point(1125 + AppLocation.X, 306 + AppLocation.Y);
            ConfirmButton = new Point(707 + AppLocation.X, 626 + AppLocation.Y);
            PurchaseButton = new Point(696 + AppLocation.X, 632 + AppLocation.Y);
            JadeRectAfter = new Rectangle(1045 + AppLocation.X, 72 + AppLocation.Y, 70, 35);

            yBase = 360;
            yIncr = 40;
            xBase = 535;
            ConfirmRect = new Rectangle(AppLocation.X + 638, AppLocation.Y + 609, 136, 34);
            DoneRect = new Rectangle(AppLocation.X + 413, AppLocation.Y + 889, 48, 27);

            com = new Common(appLocation, mainForm);
            main = mainForm;
        }

        public void AutoGift(int nPos, bool bSave)
        {

            DateTime initialTime = DateTime.Now;
            main.ClearLog();
            main.Log("AutoGift. Start time: " + initialTime.ToString("[dd_MM]H_mm_ss.fff"));
            GotoTheLeft(10);
            Thread.Sleep(2000);
            for (int i = 0; i < 600; i++)
            {
                if (CheckMerchant())
                {
                    if (i == 0)
                        main.Log("AutoGift. Merchant not finish. ");
                    Thread.Sleep(1000);
                }
                else
                    break;
            }
            main.Log("AutoGift. Merchant finished. ");
            Thread.Sleep(2000);

            for (int i = 0; i < maxLoop; i++)
            {
                if (main.IsStopped) return;
                Opening();
                Closing(6000);

                if (CheckMerchant())
                {
                    main.Log("AutoGift. Merchant!!!!! ");
                    com.PlaySound(1);
                    break;
                }
                else
                {
                    main.Log("AutoGift. No Merchant! ");
                }

            }
            bool bSuccess = BuyMerchant(nPos);
            CheckFlagToSave();
            com.PlaySound();
            double totalMinutes = DateTime.Now.Subtract(initialTime).TotalMinutes;
            if (bSave)
            {
                Bitmap JadeImageAfter = WinAPI.GetRectImage(JadeRectAfter);
                int JadeAfter = pi.GetNumberFromImage(JadeImageAfter);
                if (JadeAfter < 1000)
                {
                    com.SwitchTab();
                    if (bSuccess && totalMinutes > 2)
                        com.SendMessage("AutoGift. Saved Done.");
                }
            }
            main.SaveLog(initialTime.ToString("[dd_MM]H_mm_ss.fff") + "_Gift.txt");
            main.Log("AutoGift. Saved log file. ");
        }

        public void GotoTheLeft(int nTime)
        {
            if (main.IsStopped) return;
            main.Log("GotoTheLeft. Begin");
            try
            {
                for (int i = 0; i < nTime; i++)
                    WinAPI.DragAndDrop(DragFirst, DragSecond, 1000);
            }
            catch (Exception ex)
            {
                main.Log("GotoTheLeft. Ex: " + ex.Message.ToString());
            }
        }

        public bool CheckMerchant()
        {
            if (main.IsStopped) return false;
            //main.Log("CheckMerchant. Begin");
            try
            {
                Bitmap GiftImage = WinAPI.GetRectImage(GiftRect, false);
                Color leftTop = GiftImage.GetPixel(PointToCheck.X, PointToCheck.Y);
                //main.Log(string.Format("GotoTheLeft. R:{0}, G:{1}, B:{2} ", leftTop.R, leftTop.G, leftTop.B));
                if (leftTop.B < 100)
                    return true;
            }
            catch (Exception ex)
            {
                main.Log("CheckMerchant. Ex: " + ex.Message.ToString());
            }
            return false;
        }

        public bool BuyMerchant(int nPos) //nPos =1 or 2
        {
            if (main.IsStopped) return false;
            main.Log("BuyMerchant. Begin");
            try
            {
                WinAPI.LeftClick(PointToCheck, 1500);
                WinAPI.LeftClick(PointToCheck, 1500);
                WinAPI.LeftClick(GiftPos[nPos - 1], 1500);
                WinAPI.LeftClick(PurchaseButton, 1500);
                //for (int i = 0; i < 10; i++)
                //{
                //    WinAPI.LeftClick(new Point(xBase, yBase + yIncr * i), 500);
                //    Bitmap ConfirmImage = WinAPI.GetRectImage(ConfirmRect);
                //    bool bString = pi.CheckStringFromImage(ConfirmImage, "confirm", 200, true, true, true, 80);
                //    if (bString)
                //    {
                //        com.SendMessage("AutoGift. Buy Merchant at slot: " + nPos.ToString() + " done!");
                //        break;
                //    }
                //}
                com.SendMessage("AutoGift. Buy Merchant at slot: " + nPos.ToString() + " done! waiting for command to save..........");
                WinAPI.LeftClick(ConfirmButton, 1000);
                WinAPI.LeftClick(XButton, 3000);
                return true;
            }
            catch (Exception ex)
            {
                main.Log("BuyMerchant. Ex: " + ex.Message.ToString());
            }
            return false;
        }

        public void CheckFlagToSave()
        {
            if (main.IsStopped) return;
            main.Log("CheckFlagToSave. Begin");
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    //Bitmap DoneImage = new Bitmap(@"D:\8. Games\SecretKingdom\gift\confirmmerchant.jpg");
                    Bitmap DoneImage = WinAPI.GetRectImage(DoneRect);
                    bool bString = pi.CheckStringFromImage(DoneImage, "done", 1000, false, false, true);
                    if (bString)
                        break;
                }
            }
            catch (Exception ex)
            {
                main.Log("CheckFlagToSave. Ex: " + ex.Message.ToString());
            }
            return;
        }

        private void Opening()
        {
            if (main.IsStopped) return;
            main.Log("Opening. Begin");
            try
            {
                for (int i = 0; i < 20; i++)
                    WinAPI.LeftClick(GatePos, 400);
                WinAPI.LeftClick(StartButton, 5000);
                WinAPI.LeftClick(Speed, 21000);

                for (int i = 0; i < 20; i++)
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

    }
}
>>>>>>> 1c4e99af892a85c714c074b198f441d969c56cef
