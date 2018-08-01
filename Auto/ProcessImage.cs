using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;

namespace Auto
{
    public class ProcessImage
    {
        private Tesseract ocr;
        private string errorPath;
        IPPSearching iSearch = new IPPSearching(0);

        Auto main;
        public ProcessImage(Auto mainForm)
        {
            ocr = new Tesseract("", "eng", Tesseract.OcrEngineMode.OEM_TESSERACT_CUBE_COMBINED);
            errorPath = "D:\\8. Games\\SecretKingdom\\ImgError\\";
            main = mainForm;
        }

        public int GetNumberFromImage(Bitmap Img, bool bBinarize = true, int nThres = 215)
        {
            if (main.IsStopped) return -1;
            //main.Log("GetNumberFromImage. Begin");
            int number = 0;
            string sText;
            try
            {
                Image<Gray, byte> emguImage = new Image<Gray, Byte>(Img);
                //emguImage.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + "_origin.bmp");
                Image<Gray, byte> thresImage = emguImage.ThresholdBinary(new Gray(nThres), new Gray(255));
                //thresImage.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + "_thres.bmp");
                ocr.Recognize(thresImage);
                sText = ocr.GetText();
                sText = FixString(sText);
                sText = FixNumber(sText);
                main.Log("GetNumberFromImage. Fix sText = " + sText);
                //Img.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + ".bmp", ImageFormat.Bmp);
                number = int.Parse(sText);
                emguImage.Dispose();
            }
            catch (Exception ex)
            {
                main.Log("GetNumberFromImage. Error. " + ex.Message.ToString());
                Img.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + ".bmp", ImageFormat.Bmp);
                number = -1;
            }
            //main.Log("GetNumberFromImage. End");
            return number;
        }

        private string FixNumber(string input)
        {
            input = input.Replace('I', '1');
            input = input.Replace('i', '1');
            input = input.Replace('l', '1');
            input = input.Replace('j', '1');
            input = input.Replace('J', '1');
            input = input.Replace('O', '0');
            input = input.Replace('o', '0');
            input = input.Replace('b', '8');
            input = input.Replace("m", "11");
            // input = new String(input.Where(Char.IsDigit).ToArray());
            return input;
        }


        public bool CheckStringFromImage(Bitmap Img, string result, int sleep, bool bLog = true, bool bPreProcess = false, bool bSaveImage = false, int nThres = 245)
        {
            if (main.IsStopped) return false;
            //main.Log("CheckStringFromImage. Begin");
            string sText;
            bool bResult = false;
            Thread.Sleep(sleep);
            try
            {
                Image<Gray, byte> emguImage = new Image<Gray, Byte>(Img);
                if (bPreProcess)
                {
                    Image<Gray, byte> thresImage = emguImage.ThresholdBinary(new Gray(nThres), new Gray(255));
                    if (bSaveImage)
                        thresImage.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + "_thres.bmp");
                    ocr.Recognize(thresImage);
                }
                else
                    ocr.Recognize(emguImage);

                sText = ocr.GetText();
                sText = FixString(sText);
                if (bLog)
                    main.Log("CheckStringFromImage. sText = " + sText);
                //Img.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + "_test.bmp", ImageFormat.Bmp);
                emguImage.Dispose();
                if (sText.Contains(result))
                    bResult = true;
            }
            catch (Exception ex)
            {
                main.Log("CheckStringFromImage. Error. " + ex.Message.ToString());
                if (bSaveImage)
                    Img.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + ".bmp", ImageFormat.Bmp);
            }
            //main.Log("CheckStringFromImage. End. bResult=" + bResult.ToString());
            return bResult;
        }

        private string FixString(string input)
        {
            input = input.Replace("\r\n", "");
            input = input.ToLower();
            return input;
        }
        public void Test()
        {
            Image<Hsv, byte> emguImage = new Image<Hsv, Byte>(@"D:\8. Games\SecretKingdom\SafeJade\jade3.bmp");
            emguImage.Save(@"D:\8. Games\SecretKingdom\SafeJade\jade3_hsv.bmp");
        }
        public void Save(Bitmap Img, string folder = "", string suffix = "")
        {
            try
            {
                if (folder == "")
                    Img.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + "_" + suffix + ".bmp", ImageFormat.Bmp);
                else
                    Img.Save(folder + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + "_" + suffix + ".bmp", ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                main.Log("Save. Error. " + ex.Message.ToString());
            }
        }

        public void InitSearch()
        {
            Bitmap temp = new Bitmap(@"D:\8. Games\SecretKingdom\template\pearl.bmp");
            Byte[] bTemp = BitmapToByteArray(temp);
            iSearch.AddModel(0, bTemp, temp.Size, 0, 0, 1, 1, 1, 0.1f, 60);

            temp = new Bitmap(@"D:\8. Games\SecretKingdom\template\jade.bmp");
            bTemp = BitmapToByteArray(temp);
            iSearch.AddModel(1, bTemp, temp.Size, 0, 0, 1, 1, 1, 0.1f, 60);
        }

        public int FindJade(Bitmap src)
        {
            Image<Gray, byte> emguImage = new Image<Gray, Byte>(src);
            Bitmap gray = emguImage.ToBitmap();
            //gray.Save(errorPath + DateTime.Now.ToString("[dd_MM]H_mm_ss.fff") + "_gray.bmp", ImageFormat.Bmp);
            Byte[] bGray = BitmapToByteArray(gray);
            iSearch.Execute(bGray, gray.Size);

            return iSearch.BestMatchIdx;
        }

        public Point GetJadePosition()
        {
            Point result = new Point(-1, -1);
            if (iSearch.BestMatchIdx > -1)
            {
                result.X = (int)(iSearch.ModelList[iSearch.BestMatchIdx].MatchPosition.X);
                result.Y = (int)(iSearch.ModelList[iSearch.BestMatchIdx].MatchPosition.Y);
            }
            return result;
        }

        public float GetScore()
        {
            return iSearch.ModelList[iSearch.BestMatchIdx].Score;
        }

        public byte[] BitmapToByteArray(Bitmap bitmap)
        {

            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }
    }
}
