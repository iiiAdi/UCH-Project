using System;
using System.Drawing;
using System.Windows.Forms;

namespace UCH_Project.Classes
{
    public class HoverButton : PictureBox
    {
        private Size originalSize;
        private Point originalLocation;
        private bool isInitialized = false;

        private readonly float scaleFactor = 1.1f;

        public HoverButton()
        {
            this.SizeMode = PictureBoxSizeMode.StretchImage;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;

            this.MouseEnter += HoverButton_MouseEnter;
            this.MouseLeave += HoverButton_MouseLeave;
        }

        private void InitializeOriginalBounds()
        {
            if (!isInitialized)
            {
                originalSize = this.Size;
                originalLocation = this.Location;
                isInitialized = true;
            }
        }

        private void HoverButton_MouseEnter(object? sender, EventArgs e)
        {
            InitializeOriginalBounds();

            int newWidth = (int)(originalSize.Width * scaleFactor);
            int newHeight = (int)(originalSize.Height * scaleFactor);

            int newX = originalLocation.X - (newWidth - originalSize.Width) / 2;
            int newY = originalLocation.Y - (newHeight - originalSize.Height) / 2;

            this.SetBounds(newX, newY, newWidth, newHeight);
            this.BringToFront();
        }

        private void HoverButton_MouseLeave(object? sender, EventArgs e)
        {
            if (isInitialized)
            {
                {
                    this.SetBounds(originalLocation.X, originalLocation.Y, originalSize.Width, originalSize.Height);
                }
            }
        }
    }
}
