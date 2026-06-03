using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCH_Project.Classes
{
    public class Platform : StaticObject
    {
        private Image platformImage;
        public enum PlatformType { WoodHorizontal, WoodVertical , MetalBox, MetalFloor} // Enums for easier management

        public PlatformType Type { get; private set; }
        public Platform(int x, int y, int width, int height, PlatformType type): base(x, y, width, height) // Constructor
        {
            this.Type = type;

            switch (type)
            {
                case PlatformType.WoodHorizontal:
                    platformImage = Properties.Resources.WoodFloor;
                    break;
                case PlatformType.WoodVertical:
                    platformImage = Properties.Resources.WoodWall;
                    break;
                case PlatformType.MetalBox:
                    platformImage = Properties.Resources.MetalBox;
                    break;
                case PlatformType.MetalFloor:
                    platformImage = Properties.Resources.MetalBegin;
                    break;
                default:
                    platformImage = null;
                    break;
            }
        }

        public override void Draw(Graphics g) // Draw the object
        {
            if (platformImage != null)
            {
                g.DrawImage(platformImage, X, Y, Width, Height);
            }
            else
            {
                g.FillRectangle(Brushes.Brown, X, Y, Width, Height);
            }
        }

    }
}
