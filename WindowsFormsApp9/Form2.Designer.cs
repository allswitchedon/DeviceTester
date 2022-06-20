namespace WinFormsApp2
{
    partial class Form2
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
            this.button1 = new System.Windows.Forms.Button();
            this.plotView1 = new OxyPlot.WindowsForms.PlotView();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.rb_10 = new System.Windows.Forms.RadioButton();
            this.rb_20 = new System.Windows.Forms.RadioButton();
            this.rb_50 = new System.Windows.Forms.RadioButton();
            this.rb_100 = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(380, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // plotView1
            // 
            this.plotView1.Location = new System.Drawing.Point(0, 51);
            this.plotView1.Name = "plotView1";
            this.plotView1.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView1.Size = new System.Drawing.Size(542, 217);
            this.plotView1.TabIndex = 1;
            this.plotView1.Text = "plotView1";
            this.plotView1.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView1.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView1.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 24);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(136, 21);
            this.comboBox1.TabIndex = 2;
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(154, 24);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(100, 21);
            this.comboBox2.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Device";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(189, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Axis";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(479, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Result";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(461, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Hz";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(260, 22);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(114, 23);
            this.progressBar1.TabIndex = 8;
            // 
            // rb_10
            // 
            this.rb_10.AutoSize = true;
            this.rb_10.Location = new System.Drawing.Point(260, 4);
            this.rb_10.Name = "rb_10";
            this.rb_10.Size = new System.Drawing.Size(37, 17);
            this.rb_10.TabIndex = 9;
            this.rb_10.TabStop = true;
            this.rb_10.Text = "10";
            this.rb_10.UseVisualStyleBackColor = true;
            // 
            // rb_20
            // 
            this.rb_20.AutoSize = true;
            this.rb_20.Location = new System.Drawing.Point(303, 4);
            this.rb_20.Name = "rb_20";
            this.rb_20.Size = new System.Drawing.Size(37, 17);
            this.rb_20.TabIndex = 10;
            this.rb_20.TabStop = true;
            this.rb_20.Text = "20";
            this.rb_20.UseVisualStyleBackColor = true;
            // 
            // rb_50
            // 
            this.rb_50.AutoSize = true;
            this.rb_50.Location = new System.Drawing.Point(346, 4);
            this.rb_50.Name = "rb_50";
            this.rb_50.Size = new System.Drawing.Size(37, 17);
            this.rb_50.TabIndex = 11;
            this.rb_50.TabStop = true;
            this.rb_50.Text = "50";
            this.rb_50.UseVisualStyleBackColor = true;
            // 
            // rb_100
            // 
            this.rb_100.AutoSize = true;
            this.rb_100.Location = new System.Drawing.Point(389, 4);
            this.rb_100.Name = "rb_100";
            this.rb_100.Size = new System.Drawing.Size(43, 17);
            this.rb_100.TabIndex = 12;
            this.rb_100.TabStop = true;
            this.rb_100.Text = "100";
            this.rb_100.UseVisualStyleBackColor = true;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 267);
            this.Controls.Add(this.rb_100);
            this.Controls.Add(this.rb_50);
            this.Controls.Add(this.rb_20);
            this.Controls.Add(this.rb_10);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.plotView1);
            this.Controls.Add(this.button1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form2_FormClosed);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private OxyPlot.WindowsForms.PlotView plotView1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RadioButton rb_10;
        private System.Windows.Forms.RadioButton rb_20;
        private System.Windows.Forms.RadioButton rb_50;
        private System.Windows.Forms.RadioButton rb_100;
    }
}