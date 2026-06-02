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
            // קידום הקליע במרחב
            X += SpeedX;

            // 1. בדיקה אם הקליע יצא מגבולות המסך
            if (X < 0 || X > screenBounds.Width)
            {
                this.IsDestroyed = true;
                return; // יוצאים מהפונקציה מיד כדי לחסוך משאבי חישוב
            }

            // 2. בדיקת התנגשות עם פלטפורמות ומכשולים סטטיים
            foreach (GameObject obj in worldObjects)
            {
                // מוודאים שאנחנו לא בודקים התנגשות של הקליע עם עצמו, ושהאובייקט מולנו הוא קיר או רצפה
                if (obj != this && obj is StaticObject staticObj && staticObj.IsCollidable)
                {
                    if (this.Bounds.IntersectsWith(staticObj.Bounds))
                    {
                        // ברגע שיש פגיעה בקיר, מסמנים את הקליע למחיקה ויוצאים
                        this.IsDestroyed = true;
                        return;
                    }
                }
            }
        }

        public override void Draw(Graphics g)
        {
            // ציור הקליע
            g.FillEllipse(Brushes.Orange, X, Y, Width, Height);
        }
    }
}