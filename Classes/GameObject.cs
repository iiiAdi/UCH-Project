using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace UCH_Project
{
    [Serializable]
    public abstract class GameObject
    {
        public int X { get; set; }
        public int Y { get; set; }

        public abstract void Draw(Graphics g);

        public abstract void Update(List<GameObject> worldObjects, Size screenBounds);

        public bool IsDestroyed { get; set; } = false;
    }

}