using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCH_Project.Classes
{
    [Serializable]
    public abstract class StaticObject : GameObject
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsCollidable { get; set; } = true;
        
        public StaticObject(int x, int y, int width, int height)
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
        public override void Update(List<GameObject> worldObjects, Size screenBounds)
        {

        }
    }
}
