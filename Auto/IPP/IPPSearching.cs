//@cuong update 15/2/2017: add sub-angle precision, more accuracy with pointF, fix template sample for more accurate result
//For more information about algorithms, plz refer to Sapera's functional guide
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Drawing;


namespace Auto
{
    public class Model
    {
        const IPP.Interpolation ROTATION_MODE = IPP.Interpolation.IPPI_INTER_LINEAR;  //interpolation mode for rotation
        const IPP.Interpolation RESIZE_MODE = IPP.Interpolation.IPPI_INTER_LANCZOS;  //interpolation mode for resize
        public float RADIAN_PER_DEGREE = 0.0174532F;
        public int ID;
        public int RATIO = 4;       //Candidate scale
        public float AcceptScore;   //minimum score 
        public byte[] ModelData;    //origin
        public Size ModelSize;

        public byte[,][] Samples;   //template sample
        public Size[,] SampleSizes;

        public float[] m_Rotation;
        public float[] m_Scale;
        public int m_nRotSample;
        public int m_nScaleSample;
        public float m_f1stScore;

        public float MaxRotation, MinRotation;
        public float RotationIncr;  //Rotation increment

        public float MaxScale, MinScale;
        public float ScaleIncr;  //Scale increment

        public bool LearningDone;
        public bool IsActivated;

        //Result
        public float Score;
        public float Scale;
        public float Angle;
        public PointF MatchPosition;
        public PointF[] RecResult;
        public Model(int id, float score = 0, float scale = 1, float angle = 0)
        {
            ID = id;
            LearningDone = false;
            m_f1stScore = 0;
            Score = score;
            Scale = scale;
            Angle = angle;
            MatchPosition = new PointF(0, 0);
            RecResult = new PointF[4];
            IsActivated = true;
        }

        public void fnCreateSample()
        {
            #region get background's intensity
            int nSrcStep = 4 * (1 + (ModelSize.Width - 1) / 4);
            byte bBackground = (byte)((ModelData[0] + ModelData[ModelSize.Width - 1] + ModelData[nSrcStep * (ModelSize.Height - 1)] + ModelData[nSrcStep * (ModelSize.Height - 1) + ModelSize.Width - 1]) / 4);
            #endregion

            for (int i = 0; i < m_nRotSample; i++)
            {
                if (ModelSize.Width < ModelSize.Height)
                {
                    float nAngle = Math.Abs(m_Rotation[i]);
                    Size szOutput;
                    byte[] rotation;
                    if (nAngle <= 45)
                    {
                        double sa = Math.Sin(nAngle * RADIAN_PER_DEGREE);
                        double ca = Math.Cos(nAngle * RADIAN_PER_DEGREE);
                        int nNewHeight = (int)(((double)ModelSize.Height - (double)ModelSize.Width * sa) / ca);
                        nNewHeight = nNewHeight - ((ModelSize.Height - nNewHeight) % 2);
                        szOutput = new Size(ModelSize.Width, nNewHeight);
                    }
                    else
                    {
                        double sa = Math.Sin((90 - nAngle) * RADIAN_PER_DEGREE);
                        double ca = Math.Cos((90 - nAngle) * RADIAN_PER_DEGREE);
                        int nNewWidth = (int)(((double)ModelSize.Height - (double)ModelSize.Width * sa) / ca);
                        nNewWidth = nNewWidth - ((ModelSize.Width - nNewWidth) % 2);
                        szOutput = new Size(nNewWidth, ModelSize.Width);

                    }

                    IPP.fnRotateCenter(ModelData, ModelSize, out rotation, szOutput, m_Rotation[i], bBackground, ROTATION_MODE);

                    for (int j = 0; j < m_nScaleSample; j++)
                    {
                        int nX = (int)((float)szOutput.Width * m_Scale[j]);
                        int nY = (int)((float)szOutput.Height * m_Scale[j]);
                        SampleSizes[i, j] = new Size(nX, nY);
                        IPP.fnResize(rotation, szOutput, m_Scale[j], SampleSizes[i, j], out Samples[i, j], RESIZE_MODE);
                    }
                }
                else
                {
                    float nAngle = Math.Abs(m_Rotation[i]);
                    Size szOutput;
                    byte[] rotation;
                    if (nAngle <= 45)
                    {
                        double sa = Math.Sin(nAngle * RADIAN_PER_DEGREE);
                        double ca = Math.Cos(nAngle * RADIAN_PER_DEGREE);
                        int nNewWidth = (int)(((double)ModelSize.Width - (double)ModelSize.Height * sa) / ca);
                        nNewWidth = nNewWidth - ((ModelSize.Width - nNewWidth) % 2);
                        szOutput = new Size(nNewWidth, ModelSize.Height);
                    }
                    else
                    {
                        double sa = Math.Sin((90 - nAngle) * RADIAN_PER_DEGREE);
                        double ca = Math.Cos((90 - nAngle) * RADIAN_PER_DEGREE);
                        int nNewHeight = (int)(((double)ModelSize.Width - (double)ModelSize.Height * sa) / ca);
                        nNewHeight = nNewHeight - ((ModelSize.Height - nNewHeight) % 2);
                        szOutput = new Size(ModelSize.Height, nNewHeight);

                    }
                    IPP.fnRotateCenter(ModelData, ModelSize, out rotation, szOutput, m_Rotation[i], bBackground, ROTATION_MODE);

                    for (int j = 0; j < m_nScaleSample; j++)
                    {
                        int nX = (int)((float)szOutput.Width * m_Scale[j]);
                        int nY = (int)((float)szOutput.Height * m_Scale[j]);
                        SampleSizes[i, j] = new Size(nX, nY);
                        IPP.fnResize(rotation, szOutput, m_Scale[j], SampleSizes[i, j], out Samples[i, j], RESIZE_MODE);
                    }
                }
            }
        }
    }
    public class IPPSearching  //This class support only 1 match's location
    {
        public int NumOfModel;
        public List<Model> ModelList;  //result's score
        public int BestMatchIdx;

        //Future development
        //private int Xvicinity, Yvicinity;  //minimum region around matches wherein no other matchers can be found
        //private int MaxCandidates;  //maximum number of candidates to pass to the second phase of the search
        //private int MaxMatches;  //maximum number of matches
        //private bool SubPixel; //enable subpixel accuracy

        public IPPSearching(int nThread)
        {
            NumOfModel = 0;
            ModelList = new List<Model>();
            //IPP.ippSetNumThreads(1);
        }

        public void AddModel(int id, byte[] model, Size szModel, float minRot, float maxRot, float rotIncr,
            float minScale, float maxScale, float scaleIncr, float acceptScore = 50, int maxCan = 1)
        {
            Model temp = new Model(id);
            if (szModel.Width < 40 || szModel.Height < 40) temp.RATIO = 2;
            if (szModel.Width < 10 || szModel.Height < 10) temp.RATIO = 1;
            if (minRot < -90) minRot = -90;  //only support rotation from -90 to 90
            if (maxRot > 90) maxRot = 90;

            //Rotation
            if (maxRot > minRot && rotIncr != 0)
            {
                temp.m_nRotSample = (int)(1 + (maxRot - minRot) / rotIncr);
                temp.m_Rotation = new float[temp.m_nRotSample];
                for (int i = 0; i < temp.m_nRotSample; i++)
                    temp.m_Rotation[i] = minRot + i * rotIncr;
            }
            else
            {
                temp.m_nRotSample = 1;
                temp.m_Rotation = new float[1] { minRot };
            }

            //Scale
            if (maxScale > minScale && scaleIncr != 0)
            {
                temp.m_nScaleSample = (int)(1 + (maxScale - minScale) / scaleIncr);
                temp.m_Scale = new float[temp.m_nScaleSample];
                for (int i = 0; i < temp.m_nScaleSample; i++)
                    temp.m_Scale[i] = minScale + i * scaleIncr;
            }
            else
            {
                temp.m_nScaleSample = 1;
                temp.m_Scale = new float[1] { minScale };
            }


            temp.Samples = new byte[temp.m_nRotSample, temp.m_nScaleSample][];
            temp.SampleSizes = new Size[temp.m_nRotSample, temp.m_nScaleSample];
            temp.m_f1stScore = 0;

            temp.ModelData = model;
            temp.ModelSize = szModel;

            temp.MaxRotation = maxRot;
            temp.MinRotation = minRot;
            temp.RotationIncr = rotIncr;
            temp.MaxScale = maxScale;
            temp.MinScale = minScale;
            temp.ScaleIncr = scaleIncr;
            temp.AcceptScore = acceptScore;
            temp.fnCreateSample();
            temp.LearningDone = true;

            NumOfModel++;
            ModelList.Add(temp);
        }
        public void Execute(byte[] src, Size szSrc)
        {
            float fBestScore = 0;
            BestMatchIdx = -1;
            for (int n = 0; n < NumOfModel; n++)
            {
                ModelList[n].m_f1stScore = 0;
                ModelList[n].Score = 0;
                if (!ModelList[n].IsActivated)
                    continue;

                double dSCALE = (double)1 / (double)ModelList[n].RATIO;

                //Resize source by RATIO
                int nXsrc = (int)((double)szSrc.Width * dSCALE);
                int nYsrc = (int)((double)szSrc.Height * dSCALE);

                byte[] srcRes;
                Size szSrcRes = new Size(nXsrc, nYsrc);
                IPP.fnResize(src, szSrc, dSCALE, szSrcRes, out srcRes, IPP.Interpolation.IPPI_INTER_SUPER);


                for (int i = 0; i < ModelList[n].m_nRotSample; i++)
                {
                    for (int j = 0; j < ModelList[n].m_nScaleSample; j++)
                    {
                        //Resize template by RATIO
                        byte[] tmpRes;
                        int nXtmp = (int)((double)ModelList[n].SampleSizes[i, j].Width * dSCALE);
                        int nYtmp = (int)((double)ModelList[n].SampleSizes[i, j].Height * dSCALE);
                        Size szTmpRes = new Size(nXtmp, nYtmp);
                        IPP.fnResize(ModelList[n].Samples[i, j], ModelList[n].SampleSizes[i, j], dSCALE, szTmpRes, out tmpRes, IPP.Interpolation.IPPI_INTER_SUPER);

                        //Calculate NCC of first phase
                        PointF firstPos;
                        float fFirstScore = fnTM_1stPhase_Gray(srcRes, szSrcRes, tmpRes, szTmpRes, out firstPos);
                        if (100 * fFirstScore < 0.95 * ModelList[n].AcceptScore || fFirstScore < 0.95 * ModelList[n].m_f1stScore) continue;

                        float fFinalScore = fFirstScore;
                        PointF finalPos = firstPos;
                        ModelList[n].m_f1stScore = fFirstScore;

                        //Calculate NCC of second phase
                        if (dSCALE == 1)
                        {
                            if (100 * fFinalScore < ModelList[n].Score) continue;
                        }
                        else
                        {
                            fFinalScore = fnTM_2ndPhase_Gray(n, src, szSrc, ModelList[n].Samples[i, j], ModelList[n].SampleSizes[i, j], firstPos, out finalPos);
                            if (100 * fFinalScore < ModelList[n].AcceptScore) continue;
                        }

                        //Save result
                        ModelList[n].Score = 100 * fFinalScore;
                        ModelList[n].MatchPosition = new PointF(finalPos.X + (float)ModelList[n].SampleSizes[i, j].Width / (float)2, finalPos.Y + (float)ModelList[n].SampleSizes[i, j].Height / (float)2);  //center
                        //MatchPosition = new PointF(finalPos.X , finalPos.Y);  //center
                        ModelList[n].Angle = ModelList[n].m_Rotation[i];

                        ModelList[n].Scale = ModelList[n].m_Scale[j];
                        if (fFinalScore > fBestScore)
                        {
                            fBestScore = fFinalScore;
                            BestMatchIdx = n;
                        }
                    }
                }
            }

            if (BestMatchIdx > -1)
            {
                //Get rectangle
                float x0 = ModelList[BestMatchIdx].MatchPosition.X;
                float y0 = ModelList[BestMatchIdx].MatchPosition.Y;

                float x1 = x0 - ((float)ModelList[BestMatchIdx].ModelSize.Width / (float)2) * ModelList[BestMatchIdx].Scale;
                float y1 = y0 - ((float)ModelList[BestMatchIdx].ModelSize.Height / (float)2) * ModelList[BestMatchIdx].Scale;
                float x2 = x0 + ((float)ModelList[BestMatchIdx].ModelSize.Width / (float)2) * ModelList[BestMatchIdx].Scale;
                float y2 = y0 + ((float)ModelList[BestMatchIdx].ModelSize.Height / (float)2) * ModelList[BestMatchIdx].Scale;

                // Coordinates of the rotated rectangle
                double ca = Math.Cos(ModelList[BestMatchIdx].Angle * ModelList[BestMatchIdx].RADIAN_PER_DEGREE);
                double sa = Math.Sin(ModelList[BestMatchIdx].Angle * ModelList[BestMatchIdx].RADIAN_PER_DEGREE);
                double rx1 = (x0 + (x1 - x0) * ca - (y1 - y0) * sa);
                double ry1 = (y0 + (x1 - x0) * sa + (y1 - y0) * ca);
                double rx2 = (x0 + (x2 - x0) * ca - (y1 - y0) * sa);
                double ry2 = (y0 + (x2 - x0) * sa + (y1 - y0) * ca);
                double rx3 = (x0 + (x2 - x0) * ca - (y2 - y0) * sa);
                double ry3 = (y0 + (x2 - x0) * sa + (y2 - y0) * ca);
                double rx4 = (x0 + (x1 - x0) * ca - (y2 - y0) * sa);
                double ry4 = (y0 + (x1 - x0) * sa + (y2 - y0) * ca);

                ModelList[BestMatchIdx].RecResult[0] = new PointF((float)rx1, (float)ry1);
                ModelList[BestMatchIdx].RecResult[1] = new PointF((float)rx2, (float)ry2);
                ModelList[BestMatchIdx].RecResult[2] = new PointF((float)rx3, (float)ry3);
                ModelList[BestMatchIdx].RecResult[3] = new PointF((float)rx4, (float)ry4);
            }
        }

        private float fnTM_1stPhase_Gray(byte[] srcRes, Size szSrcRes, byte[] tmpRes, Size szTmpRes, out PointF firstPos)
        {
            float fScore = 0;
            int xPosRes = 0, yPosRes = 0;

            Size sz1stPhase = szSrcRes - szTmpRes + new Size(1, 1);
            float[] bmp1stPhase = new float[sz1stPhase.Width * sz1stPhase.Height];

            int nSrcResStep = 4 * (1 + (szSrcRes.Width - 1) / 4);
            int nDstResStep = 4 * (1 + (szTmpRes.Width - 1) / 4);
            unsafe
            {
                fixed (float* tbmpDst = bmp1stPhase)
                fixed (byte* pSrcRes = srcRes)
                fixed (byte* pTmpRes = tmpRes)
                {
                    IPP.ippiCrossCorrValid_NormLevel_8u32f_C1R(pSrcRes, nSrcResStep, szSrcRes, pTmpRes, nDstResStep, szTmpRes, tbmpDst, 4 * sz1stPhase.Width);
                    IPP.ippiMaxIndx_32f_C1R(tbmpDst, 4 * sz1stPhase.Width, sz1stPhase, &fScore, &xPosRes, &yPosRes);
                }
            }

            firstPos = new PointF(xPosRes, yPosRes);
            return fScore;
        }
        private float fnTM_2ndPhase_Gray(int idx, byte[] src, Size szSrc, byte[] tmp, Size szTmp, PointF firstPos, out PointF finalPos)
        {
            float fFinalScore = 0;
            Point leftTop = new Point(0, 0);
            Point rightBot = new Point(0, 0);

            leftTop.X = (int)Math.Max(ModelList[idx].RATIO * firstPos.X - ModelList[idx].RATIO / 2, 0);
            leftTop.Y = (int)Math.Max(ModelList[idx].RATIO * firstPos.Y - ModelList[idx].RATIO / 2, 0);

            rightBot.X = (int)Math.Min(ModelList[idx].RATIO * firstPos.X + ModelList[idx].RATIO / 2 + szTmp.Width, szSrc.Width - 1);
            rightBot.Y = (int)Math.Min(ModelList[idx].RATIO * firstPos.Y + ModelList[idx].RATIO / 2 + szTmp.Height, szSrc.Height - 1);

            Size szSrcCrop = new Size(rightBot.X - leftTop.X + 1, rightBot.Y - leftTop.Y + 1);

            byte[] srcCrop;
            IPP.fnCrop(src, szSrc, new Rectangle(leftTop, szSrcCrop), out srcCrop);

            Size szDstCrop = new Size(rightBot.X - leftTop.X - szTmp.Width + 1, rightBot.Y - leftTop.Y - szTmp.Height + 1);
            if (szDstCrop.Width < 1 || szDstCrop.Height < 1)
            {
                finalPos = new PointF(0, 0);
                return 0F;
            }
            float[] bmpDstCrop = new float[szDstCrop.Width * szDstCrop.Height];
            int xPos = 0, yPos = 0;

            int nDstStep = 4 * (1 + (szTmp.Width - 1) / 4);
            int nSrcCropStep = 4 * (1 + (szSrcCrop.Width - 1) / 4);
            unsafe
            {
                fixed (float* tbmpDst = bmpDstCrop)
                fixed (byte* pSrc = srcCrop)
                fixed (byte* pTmp = tmp)
                {
                    IPP.ippiCrossCorrValid_NormLevel_8u32f_C1R(pSrc, nSrcCropStep, szSrcCrop, pTmp, nDstStep, szTmp, tbmpDst, 4 * szDstCrop.Width);
                    IPP.ippiMaxIndx_32f_C1R(tbmpDst, 4 * szDstCrop.Width, szDstCrop, &fFinalScore, &xPos, &yPos);
                }
            }

            //finalPos = new PointF(xPos + RATIO * firstPos.X - RATIO / 2, yPos + RATIO * firstPos.Y - RATIO / 2);
            finalPos = new PointF(xPos + leftTop.X, yPos + leftTop.Y);
            return fFinalScore;
        }
    }
}
