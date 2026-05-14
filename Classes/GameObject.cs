using System;
using System.Collections.Generic;
using System.Text;

namespace UCH_Project
{
    public abstract class GameObject
    {
        public int X { get; set; }
        public int Y { get; set; }

        public abstract void Draw(Graphics g);
    }
}
