using System;
using System.Drawing;
using System.Windows.Forms;

namespace UCH_Project.Classes
{
    public class HoverButton : PictureBox
    {
        private System.Windows.Forms.Timer animationTimer;
        private Size targetSize;
        private Point targetLocation;
        private Size originalSize;
        private Point originalLocation;
        private readonly float scaleFactor = 1.2f;
        private readonly int step = 4;

        public HoverButton()
        {
            this.SizeMode = PictureBoxSizeMode.StretchImage;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 15;
            animationTimer.Tick += AnimationTimer_Tick;

            this.MouseEnter += (s, e) => StartAnimation(true);
            this.MouseLeave += (s, e) => StartAnimation(false);
        }

        private void StartAnimation(bool growing)
        {
            if (originalSize.Width == 0)
            {
                originalSize = this.Size;
                originalLocation = this.Location;
            }

            if (growing)
            {
                int newWidth = (int)(originalSize.Width * scaleFactor);
                int newHeight = (int)(originalSize.Height * scaleFactor);
                targetSize = new Size(newWidth, newHeight);
                targetLocation = new Point(originalLocation.X - (newWidth - originalSize.Width) / 2,
                                          originalLocation.Y - (newHeight - originalSize.Height) / 2);
            }
            else
            {
                targetSize = originalSize;
                targetLocation = originalLocation;
            }

            this.BringToFront();
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            int widthDiff = targetSize.Width - this.Width;
            int heightDiff = targetSize.Height - this.Height;

            if (Math.Abs(widthDiff) < step && Math.Abs(heightDiff) < step)
            {
                this.SetBounds(targetLocation.X, targetLocation.Y, targetSize.Width, targetSize.Height);
                animationTimer.Stop();
                return;
            }

            int dirW = widthDiff > 0 ? step : -step;
            int dirH = heightDiff > 0 ? step : -step;

            this.SetBounds(
                this.Left - (dirW / 2),
                this.Top - (dirH / 2),
                this.Width + dirW,
                this.Height + dirH
            );
        }

        private bool ApplyStep()
        {
            int widthDiff = targetSize.Width - this.Width;
            int heightDiff = targetSize.Height - this.Height;

            if (Math.Abs(widthDiff) < step && Math.Abs(heightDiff) < step)
            {
                this.Size = targetSize;
                this.Location = targetLocation;
                return true;
            }

            int newWidth = this.Width + (widthDiff > 0 ? step : -step);
            int newHeight = this.Height + (heightDiff > 0 ? step : -step);

            this.Size = new Size(newWidth, newHeight);
            this.Location = new Point(this.Location.X - (newWidth - this.Width) / 2,
                                      this.Location.Y - (newHeight - this.Height) / 2);

            return false;
        }
    }
}