using System;
using System.Collections.Generic;
using System.Drawing;

namespace UCH_Project.Classes
{
    public abstract class ActionObject : GameObject
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public int SpeedX { get; set; }
        public int SpeedY { get; set; }

        public ActionObject(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(X, Y, Width, Height); }
        }

        
        public abstract override void Update(List<GameObject> worldObjects, Size screenBounds);
    }
}