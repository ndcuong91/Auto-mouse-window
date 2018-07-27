using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

using System.IO;
using System.Drawing.Imaging;

namespace Auto
{
    public class IPP
    {
        const float RADIAN_PER_DEGREE = 0.0174532F;
        public enum Interpolation
        {
            IPPI_INTER_NN = 1,                      //Nearest neighbor interpolation. 1
            IPPI_INTER_LINEAR,                      //Linear interpolation. 2
            IPPI_INTER_CUBIC,                       //Cubic interpolation. 3
            IPPI_INTER_LANCZOS,                     //Interpolation using 3-lobed Lanczos window function. 4
            IPPI_INTER_CUBIC2P_BSPLINE,             //Interpolation using B-spline. 5
            IPPI_INTER_CUBIC2P_CATMULLROM,          //Interpolation using Catmull-Rom spline. 6
            IPPI_INTER_CUBIC2P_B05C03,              //Interpolation using special cubic filter. 7
            IPPI_INTER_SUPER                        //Supersampling interpolation. 8
        }

        public static int Step(int nWidth, int nByte = 1)
        {
            return 4 * (1 + (nByte * nWidth - 1) / 4);
        }

        public static void fnResize(byte[] input, Size szInput, double dScale, Size szOutput, out byte[] output, Interpolation mode = IPP.Interpolation.IPPI_INTER_SUPER)
        {
            Rectangle recSrc = new Rectangle(0, 0, szInput.Width, szInput.Height);
            int nSrcStep = Step(szInput.Width);
            int nDstStep = Step(szOutput.Width);
            output = new byte[nDstStep * szOutput.Height];
            if (dScale == 1)
            {
                output = input;
                return;
            }
            unsafe
            {
                fixed (byte* pInput = input)
                fixed (byte* pOutput = output)
                {
                    ippiResize_8u_C1R(pInput, szInput, nSrcStep, recSrc, pOutput, nDstStep, szOutput, dScale, dScale, mode);
                }
            }
        }

        public static void fnRotateCenter(byte[] input, Size szInput, out byte[] output, Size szOutput, double dAngle, byte bBackground, Interpolation mode = Interpolation.IPPI_INTER_LINEAR)
        {
            int nByte = input.GetLength(0) / (szInput.Width * szInput.Height);
            int nSrcStep = Step(szInput.Width, nByte);
            double dXcenter = (double)(szInput.Width - 1) / (double)2;
            double dYcenter = (double)(szInput.Height - 1) / (double)2;

            byte[] input_rotate = new byte[nSrcStep * szInput.Height];
            Rectangle recSrc = new Rectangle(0, 0, szInput.Width, szInput.Height);

            if (dAngle == 0)
            {
                output = input;
                return;
            }

            unsafe
            {
                fixed (byte* pInput = input)
                fixed (byte* pOutput = input_rotate)
                {
                    if (nByte == 1)
                    {
                        ippiSet_8u_C1R(bBackground, pOutput, nSrcStep, szInput);
                        ippiRotateCenter_8u_C1R(pInput, szInput, nSrcStep, recSrc, pOutput, nSrcStep, recSrc, -dAngle, dXcenter, dYcenter, (int)mode);
                    }
                    else
                    {
                        ippiSet_8u_C3R(bBackground, pOutput, nSrcStep, szInput);
                        ippiRotateCenter_8u_C3R(pInput, szInput, nSrcStep, recSrc, pOutput, nSrcStep, recSrc, -dAngle, dXcenter, dYcenter, (int)mode);
                    }
                }
            }

            Rectangle recDst = new Rectangle((szInput.Width - szOutput.Width) / 2, (szInput.Height - szOutput.Height) / 2, szOutput.Width, szOutput.Height);
            fnCrop(input_rotate, szInput, recDst, out output);
        }

        public static void fnRotateCenterGray3(byte[] input, Size sz, out byte[] output, Size szOutput, double dAngle, byte bBackground)
        {
            int nSrcStep = 4 * (1 + (1 * sz.Width - 1) / 4);
            double dXcenter = (double)(sz.Width - 1) / (double)2;
            double dYcenter = (double)(sz.Height - 1) / (double)2;

            int nDstStep = 4 * (1 + (1 * szOutput.Width - 1) / 4);
            output = new byte[nDstStep * szOutput.Height];
            Rectangle recSrc = new Rectangle(0, 0, sz.Width, sz.Height);
            //Rectangle recSrc = new Rectangle((sz.Width - szOutput.Width) / 2, (sz.Height - szOutput.Height) / 2, szOutput.Width, szOutput.Height);
            //Rectangle recDst = new Rectangle((sz.Width - szOutput.Width) / 2, (sz.Height - szOutput.Height) / 2, szOutput.Width, szOutput.Height);
            Rectangle recDst = new Rectangle(0, 0, szOutput.Width, szOutput.Height);

            if (dAngle == 0)
            {
                output = input;
                return;
            }

            unsafe
            {
                fixed (byte* pInput = input)
                fixed (byte* pOutput = output)
                {
                    ippiSet_8u_C1R(bBackground, pOutput, nDstStep, szOutput);
                    ippiRotateCenter_8u_C1R(pInput, sz, nSrcStep, recSrc, pOutput, nDstStep, recDst, -dAngle, dXcenter, dYcenter, 2);

                    //ippiSet_8u_C3R(bBackground, pOutput, nSrcStep, szOutput);
                    //ippiRotateCenter_8u_C3R(pInput, sz, nSrcStep, recSrc, pOutput, nDstStep, recDst, -dAngle, dXcenter, dYcenter, 2);
                }
            }

        }

        public static void fnRotate_Shift(byte[] input, Size sz, out byte[] output, Size szOutput, double dAngle, double dXshift, double dYshift, byte bBackground)
        {
            int nByte = input.GetLength(0) / (sz.Width * sz.Height);
            int nSrcStep = nByte * (4 * (1 + (sz.Width - 1) / 4));

            double sa = Math.Sin(dAngle * RADIAN_PER_DEGREE);
            double ca = Math.Cos(dAngle * RADIAN_PER_DEGREE);

            int nDstStep = nByte * (4 * (1 + (szOutput.Width - 1) / 4));
            output = new byte[nDstStep * szOutput.Height];
            Rectangle recSrc = new Rectangle(0, 0, sz.Width, sz.Height);
            Rectangle recDst = new Rectangle(0, 0, szOutput.Width, szOutput.Height);

            unsafe
            {
                fixed (byte* pInput = input)
                fixed (byte* pOutput = output)
                {
                    if (nByte == 1)
                    {
                        ippiSet_8u_C1R(bBackground, pOutput, nDstStep, szOutput);
                        ippiRotate_8u_C1R(pInput, sz, nSrcStep, recSrc, pOutput, nDstStep, recDst, -dAngle, dXshift + (double)(szOutput.Width - sz.Width) / (double)2, dYshift + (double)(szOutput.Height - sz.Height - 1) / (double)2, 4);
                    }
                    else
                    {
                        ippiSet_8u_C3R(bBackground, pOutput, nDstStep, szOutput);
                        ippiRotate_8u_C3R(pInput, sz, nSrcStep, recSrc, pOutput, nDstStep, recDst, -dAngle, dXshift + (double)(szOutput.Width - sz.Width) / (double)2, dYshift + (double)(szOutput.Height - sz.Height - 1) / (double)2, 4);
                    }
                }
            }
        }

        public static void Convert(byte[,] input, Size szInput, int nByte, out byte[] output)
        {
            int nInputStep = Step(szInput.Width, nByte);
            output = new byte[nInputStep * szInput.Height];
            for (int y = 0; y < szInput.Height; y++)
                Buffer.BlockCopy(input, y * nInputStep, output, y * nInputStep, nInputStep);
        }

        public static void Convert(IntPtr ptr, Size szInput, int nByte, out byte[] output)
        {
            int nInputStep = Step(szInput.Width, nByte);
            output = new byte[nInputStep * szInput.Height];
            Marshal.Copy(ptr, output, 0, nInputStep * szInput.Height);
        }

        public static void fnCrop(byte[] input, Size szInput, Rectangle ROI, out byte[] output)
        {
            if (ROI.Size == szInput)
            {
                output = input;
                return;
            }

            int nInputStep = Step(szInput.Width);
            int nOutputStep = Step(ROI.Width);
            output = new byte[nOutputStep * ROI.Height];
            for (int y = 0; y < ROI.Height; y++)
                Buffer.BlockCopy(input, ROI.X + nInputStep * (ROI.Y + y), output, y * nOutputStep, ROI.Width);
        }

        public static void fnCopy(byte[] src, int srcStep, ref byte[] dst, int dstStep, Size szROI)
        {
            unsafe
            {
                fixed (byte* pSrc = src)
                fixed (byte* pDst = dst)
                {
                    ippiCopy_8u_C1R(pSrc, srcStep, pDst, dstStep, szROI);
                }
            }
        }

        unsafe public static void fnSaveImg(byte[] input, Size szInput, string sName)
        {
            int nByte = input.GetLength(0) / (szInput.Width * szInput.Height);

            //Bitmap bmp = new Bitmap(szInput.Width, szInput.Height, PixelFormat.Format24bppRgb); 
            //Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            //BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            //IntPtr ptr = bmpData.Scan0;
            //Marshal.Copy(input, 0, ptr, input.Length);
            //bmp.UnlockBits(bmpData);

            //Bitmap bmp;
            //fixed (byte* pInput = input)
            //{
            //    IntPtr ip = (IntPtr)(pInput);
            //    bmp = new Bitmap(szInput.Width, szInput.Height, Step(szInput.Width,3), PixelFormat.Format24bppRgb, ip);
            //}
            //bmp.Save(sName);


            if (nByte == 1)
            {
                int nSrcStep = Step(szInput.Width);
                fixed (byte* pInput = input)
                {
                    IntPtr ip = (IntPtr)(pInput);
                    //Image<Gray, byte> test = new Image<Gray, byte>(szInput.Width, szInput.Height, nSrcStep, ip);
                    //test.Save(sName);
                }
            }
            else
            {
                byte[, ,] data = new byte[szInput.Height, szInput.Width, 3];

                for (int y = 0; y < szInput.Height; y++)
                {
                    for (int x = 0; x < szInput.Width; x++)
                    {
                        data[y, x, 2] = input[y * 3 * szInput.Width + 3 * x];
                        data[y, x, 1] = input[y * 3 * szInput.Width + 3 * x + 1];
                        data[y, x, 0] = input[y * 3 * szInput.Width + 3 * x + 2];
                    }
                }
                //Image<Rgb, byte> test = new Image<Rgb, byte>(data);
                //test.Save(sName);
            }
        }

        [DllImport("ippcoreem64t-5.3.dll")]
        unsafe public static extern void ippSetNumThreads(int numThr);

        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiCopy_8u_C1R(byte* pSrc, int srcStep, byte* pDst, int dstStep, Size roiSize);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiSet_8u_C1R(byte value, byte* pDst, int dstStep, Size roiSize);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiSet_8u_C3R(byte value, byte* pDst, int dstStep, Size roiSize);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiSum_8u_C1R(byte* pSrc, int srcStep, Size roiSize, double* pSum);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiRotateCenter_8u_C1R(byte* pSrc, Size szSrc, int srcStep, Rectangle recSrc,
            byte* pDst, int dstStep, Rectangle recDst, double angle, double xCenter, double yCenter, int interpolation);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiRotateCenter_8u_C3R(byte* pSrc, Size szSrc, int srcStep, Rectangle recSrc,
            byte* pDst, int dstStep, Rectangle recDst, double angle, double xCenter, double yCenter, int interpolation);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiRotate_8u_C1R(byte* pSrc, Size szSrc, int srcStep, Rectangle recSrc,
            byte* pDst, int dstStep, Rectangle recDst, double angle, double xShift, double yShift, int interpolation);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiRotate_8u_C3R(byte* pSrc, Size szSrc, int srcStep, Rectangle recSrc,
            byte* pDst, int dstStep, Rectangle recDst, double angle, double xShift, double yShift, int interpolation);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiResize_8u_C1R(byte* pSrc, Size szSrc, int srcStep, Rectangle recSrc, byte* pDst, int dstStep, Size szDst, double xFactor, double yFactor, Interpolation interpolation);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiCrossCorrValid_NormLevel_8u32f_C1R(byte* pSrc, int srcStep, Size szSrcROI, byte* pTmp, int tmpStep, Size szTmpROI, float* pDst, int dstStep);
        [DllImport("ippsem64t-5.3.dll")]
        unsafe public static extern byte* ippsMalloc_8u(int len);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiMalloc_8u_C1(int width, int height, int* size);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiResizeGetBufSize(Rectangle srcROI, Rectangle dstROI, int nChannel, int interpolation, int* pBufferSize);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiMaxIndx_32f_C1R(float* pSrc, int srcStep, Size szSrcROI, float* pMax, int* x, int* y);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiResizeSqrPixel_8u_C1R(byte* pSrc, Size szSrcROI, int srcStep, Rectangle srcROI, byte* pDst, int dstStep, Rectangle dstROI, double xFactor, double yFactor, double xShift, double yShift, int interpolation, byte* bBuff);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiMirror_8u_C3R(byte* pSrc, int srcStep, byte* pDst, int dstStep, Size roiSize, int nFlip);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiTranspose_8u_C1R(byte* pSrc, int srcStep, byte* pDst, int dstStep, Size roiSize);
        [DllImport("ippiem64t-5.3.dll")]
        unsafe public static extern void ippiCrossCorrSame_Norm_8u32f_C1R(byte* pSrc, int srcStep, Size srcRoiSize, byte* pTpl, int tplStep, Size tplRoiSize, byte* pDst, int dstStep);
    }
}
