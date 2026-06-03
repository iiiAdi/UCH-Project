using System;
using System.Drawing;
using System.Windows.Forms;

namespace UCH_Project.Classes
{
    public class HoverButton : PictureBox // Taking for the acutal PictureBox Toolbox Item :: Poly
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

        // Initializer to save the button settings
        private void InitializeOriginalBounds()
        {
            if (!isInitialized)
            {
                originalSize = this.Size;
                originalLocation = this.Location;
                isInitialized = true;
            }
        }

        // On mouse hover, make it bigger
        private void HoverButton_MouseEnter(object? sender, EventArgs e)
        {
            InitializeOriginalBounds();

            // The bigger part
            int newWidth = (int)(originalSize.Width * scaleFactor);
            int newHeight = (int)(originalSize.Height * scaleFactor);

            // Keeping the current position so it won't get any new offsets.
            int newX = originalLocation.X - (newWidth - originalSize.Width) / 2;
            int newY = originalLocation.Y - (newHeight - originalSize.Height) / 2;

            this.SetBounds(newX, newY, newWidth, newHeight);
            this.BringToFront();
        }

        // OnMouseLeave - return everything back
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
