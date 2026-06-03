using System;
using System.Collections.Generic;
using System.Drawing;

namespace UCH_Project.Classes
{
    public class Goal : StaticObject
    {
        public Goal(int x, int y, int width, int height) : base(x, y, width, height)
        {
            // Disabling collision so it won't bother the players entering it
            this.IsCollidable = false;
        }

        public override void Draw(Graphics g)
        {
            // Green Color so it's visible
            g.FillRectangle(Brushes.Green, X, Y, Width, Height);
        }
    }
}