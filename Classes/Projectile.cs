using System.Collections.Generic;
using System.Drawing;

namespace UCH_Project.Classes
{
    public class Projectile : ActionObject
    {
        public Projectile(int x, int y, int width, int height, int speedX)
            : base(x, y, width, height)
        {
            this.SpeedX = speedX;
            this.SpeedY = 0;
        }

        public override void Update(List<GameObject> worldObjects, Size screenBounds)
        {
            // Make the projectile move right
            X += SpeedX;

            // Is the projectile still in screen bounds, if so destroy
            if (X < 0 || X > screenBounds.Width)
            {
                this.IsDestroyed = true;
                return;
            }

            // if got hit on an object
            foreach (GameObject obj in worldObjects)
            {
                // If the object is a wall or floor, then it will be destroyed.
                if (obj != this && obj is StaticObject staticObj && staticObj.IsCollidable)
                {
                    if (this.Bounds.IntersectsWith(staticObj.Bounds))
                    { 
                        this.IsDestroyed = true;
                        return;
                    }
                }
            }
        }

        public override void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.Orange, X, Y, Width, Height);
        }
    }
}