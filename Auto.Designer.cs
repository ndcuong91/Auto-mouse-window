namespace Auto
{
    partial class Auto
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonJade = new System.Windows.Forms.Button();
            this.buttonGold = new System.Windows.Forms.Button();
            this.labelFood = new System.Windows.Forms.Label();
            this.tbFood = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonJade
            // 
            this.buttonJade.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.buttonJade.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonJade.Location = new System.Drawing.Point(12, 13);
            this.buttonJade.Name = "buttonJade";
            this.buttonJade.Size = new System.Drawing.Size(96, 36);
            this.buttonJade.TabIndex = 0;
            this.buttonJade.Text = "Jade";
            this.buttonJade.UseVisualStyleBackColor = false;
            // 
            // buttonGold
            // 
            this.buttonGold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.buttonGold.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGold.Location = new System.Drawing.Point(118, 12);
            this.buttonGold.Name = "buttonGold";
            this.buttonGold.Size = new System.Drawing.Size(96, 36);
            this.buttonGold.TabIndex = 1;
            this.buttonGold.Text = "Gold";
            this.buttonGold.UseVisualStyleBackColor = false;
            // 
            // labelFood
            // 
            this.labelFood.AutoSize = true;
            this.labelFood.Location = new System.Drawing.Point(234, 24);
            this.labelFood.Name = "labelFood";
            this.labelFood.Size = new System.Drawing.Size(31, 13);
            this.labelFood.TabIndex = 2;
            this.labelFood.Text = "Food";
            // 
            // tbFood
            // 
            this.tbFood.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbFood.Location = new System.Drawing.Point(277, 20);
            this.tbFood.Name = "tbFood";
            this.tbFood.Size = new System.Drawing.Size(33, 21);
            this.tbFood.TabIndex = 3;
            this.tbFood.Text = "12";
            // 
            // Auto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 61);
            this.Controls.Add(this.tbFood);
            this.Controls.Add(this.labelFood);
            this.Controls.Add(this.buttonGold);
            this.Controls.Add(this.buttonJade);
            this.Name = "Auto";
            this.Text = "Auto";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonJade;
        private System.Windows.Forms.Button buttonGold;
        private System.Windows.Forms.Label labelFood;
        private System.Windows.Forms.TextBox tbFood;
    }
}

