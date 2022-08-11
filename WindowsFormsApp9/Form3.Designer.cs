namespace WinFormsApp2
{
    partial class Form3
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
            this.car_braking_line = new OxyPlot.WindowsForms.PlotView();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // car_braking_line
            // 
            this.car_braking_line.Location = new System.Drawing.Point(0, 0);
            this.car_braking_line.Margin = new System.Windows.Forms.Padding(2);
            this.car_braking_line.Name = "car_braking_line";
            this.car_braking_line.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.car_braking_line.Size = new System.Drawing.Size(762, 762);
            this.car_braking_line.TabIndex = 1;
            this.car_braking_line.Text = "plotView2";
            this.car_braking_line.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.car_braking_line.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.car_braking_line.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Arial", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(550, 592);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 133);
            this.label1.TabIndex = 2;
            this.label1.Text = "V 200 kph\r\nR 1000 m\r\nA 360 deg";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 763);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.car_braking_line);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form3";
            this.Text = "Braking Line";
            this.Load += new System.EventHandler(this.Form3_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public OxyPlot.WindowsForms.PlotView car_braking_line;
        private System.Windows.Forms.Label label1;
    }
}