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
            X += SpeedX;

            // אם הקליע יוצא מגבולות המסך, נסמן אותו למחיקה כדי שלא יעמיס על הזיכרון
            if (X < 0 || X > screenBounds.Width)
            {
                this.IsDestroyed = true;
            }
        }

        public override void Draw(Graphics g)
        {
            // נצייר את הקליע בכתום
            g.FillEllipse(Brushes.Orange, X, Y, Width, Height);
        }
    }
}