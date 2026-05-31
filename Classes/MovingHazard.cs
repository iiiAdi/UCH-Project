using System;
using System.Collections.Generic;
using System.Drawing;

namespace UCH_Project.Classes
{
    public class MovingHazard : ActionObject
    {
        public MovingHazard(int x, int y, int width, int height, int speedX)
            : base(x, y, width, height)
        {
            this.SpeedX = speedX;
            this.SpeedY = 0; // מוכן להרחבה עתידית אם תרצה מכשול שזז אנכית
        }

        public override void Update(List<GameObject> worldObjects, Size screenBounds)
        {
            // moving the object based on his current velocity
            X += SpeedX;

            // checking collusion with screen 
            if (X <= 0)
            {
                X = 0; // placing object on the left edge
                SpeedX *= -1; // changing direction from left to right
            }
            else if (X + Width >= screenBounds.Width)
            {
                X = screenBounds.Width - Width; // placing object on the right edge
                SpeedX *= -1; //  changing direction from right to left
            }

            // 2. checking collusion with obstacles
            foreach (GameObject obj in worldObjects)
            {
                // checking collusion of objects with themselves
                if (obj != this && obj is StaticObject staticObj && staticObj.IsCollidable)
                {
                    if (this.Bounds.IntersectsWith(staticObj.Bounds))
                    {
                        // changing moving direction
                        SpeedX *= -1;

                        
                        if (SpeedX > 0)
                        {
                         
                            X = staticObj.X + staticObj.Width;
                        }
                        else
                        {
                            X = staticObj.X - this.Width;
                        }
                    }
                }
            }

            
            Y += SpeedY;
        }

        public override void Draw(Graphics g)
        {
            // coloring the obstacle with red to emphazise the danger.
            g.FillRectangle(Brushes.Red, X, Y, Width, Height);
        }
    }
}