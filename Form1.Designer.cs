using UCH_Project.Classes;

namespace UCH_Project
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            StartButton = new HoverButton();
            ((System.ComponentModel.ISupportInitialize)StartButton).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.BackColor = Color.Transparent;
            StartButton.Image = Properties.Resources.StartButton;
            StartButton.Location = new Point(758, 513);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(231, 109);
            StartButton.SizeMode = PictureBoxSizeMode.StretchImage;
            StartButton.TabIndex = 1;
            StartButton.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1663, 767);
            Controls.Add(StartButton);
            Margin = new Padding(3, 2, 3, 2);
            MaximumSize = new Size(1679, 812);
            MinimumSize = new Size(1679, 773);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)StartButton).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private HoverButton StartButton;
    }
}
