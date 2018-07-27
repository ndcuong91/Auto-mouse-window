using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Auto
{
    public partial class Auto : Form
    {
        Jade autoJade;
        SafeJade_v2 autoSafeJade;
        Gold autoGold;
        Gift autoGift;
        //Sell autoSell;
        Scroll autoSroll;
        Ring autoRing;
        Orange autoOrange;
        Point AppLocation;
        public bool IsStopped;
        public Auto()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            IsStopped = true;
            AppLocation = WinAPI.GetAppLocation();
            autoJade = new Jade(AppLocation, this);
            autoSafeJade = new SafeJade_v2(AppLocation, this);
            autoGold = new Gold(AppLocation, this);
            autoGift = new Gift(AppLocation, this);
            autoSroll = new Scroll(AppLocation, this);
            autoRing = new Ring(AppLocation, this);
            autoOrange = new Orange(AppLocation, this);

        }

        private void buttonAuto_Click(object sender, EventArgs e)
        {
            if (IsStopped)
            {
                if (!bgrWorker.IsBusy)
                {
                    Log("Running...");
                    Thread.Sleep(500);
                    buttonAuto.BackColor = Color.FromArgb(255, 255, 128);
                    buttonAuto.Text = "Stop";
                    bgrWorker.RunWorkerAsync();  //Transfering process
                    IsStopped = false;
                }
            }
            else
            {
                Log("Stoped!");
                buttonAuto.BackColor = Color.FromArgb(255, 192, 192);
                buttonAuto.Text = "Auto";
                IsStopped = true;
            }
        }

        private void bgrWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log("bgrWorker_DoWork. Begin");
            ClearLog();
            if (checkBoxRing.Checked)
            {
                Log("bgrWorker_DoWork. AutoRing");
                autoRing.AutoRing(tbRingCode.Text);
                Thread.Sleep(1000);
            }
            if (checkBoxScroll.Checked)
            {
                Log("bgrWorker_DoWork. AutoScroll");
                autoSroll.AutoScroll(tbScrollCode.Text, checkBoxGold.Checked);
                Thread.Sleep(1000);
            }
            if (checkBoxGold.Checked)
            {
                Log("bgrWorker_DoWork. AutoGold");
                autoGold.AutoGold(int.Parse(tbFood.Text));
                Thread.Sleep(1000);
            }
            if (checkBoxJade.Checked)
            {
                Log("bgrWorker_DoWork. AutoJade");
                autoJade.AutoJade(int.Parse(tbWarning.Text));
            }
            if (checkBoxSafeJade.Checked)
            {
                Log("bgrWorker_DoWork. AutoSafeJade");
                autoSafeJade.AutoSafeJade(int.Parse(tbSafeJade.Text));
            }

            if (checkBoxGift.Checked)
            {
                Log("bgrWorker_DoWork. AutoGift");
                autoGift.AutoGift(int.Parse(tbPos.Text), checkBoxSave.Checked);
            }

            if (checkBoxOrange.Checked)
            {
                Log("bgrWorker_DoWork. AutoOrange");
                autoOrange.AutoOrange(int.Parse(tbOrange.Text));
            }

            Log("bgrWorker_DoWork. End");
            this.BeginInvoke((MethodInvoker)delegate
            {
                buttonAuto.BackColor = Color.FromArgb(255, 192, 192);
                buttonAuto.Text = "Auto";
                IsStopped = true;
            });
        }

        public void Log(string content)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                this.richTextLog.Text += content + "\n";
                this.richTextLog.SelectionStart = this.richTextLog.Text.Length;
                this.richTextLog.ScrollToCaret(); //Scroll to the end
            });
        }

        public void SaveLog(string fileName)
        {
            string logFolder = @"D:\8. Games\SecretKingdom\Log\";
            this.BeginInvoke((MethodInvoker)delegate
            {
                File.WriteAllText(logFolder + fileName, this.richTextLog.Text);
            });
        }

        public void ClearLog()
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                this.richTextLog.Text = ""; //Clear log file
            });
        }

        private void Auto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Log("press escape");
            }
        }

    }
}
