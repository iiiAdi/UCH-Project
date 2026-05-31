using System.Collections.Generic;
using System.Drawing;

namespace UCH_Project.Classes
{
    public class ProjectileTrap : ActionObject
    {
        private int cooldownCounter = 0;
        private int fireRate;
        private int projectileSpeed;

        public ProjectileTrap(int x, int y, int width, int height, int fireRate, int projectileSpeed)
            : base(x, y, width, height)
        {
            this.fireRate = fireRate;
            this.projectileSpeed = projectileSpeed;
            this.SpeedX = 0;
            this.SpeedY = 0;
        }

        public override void Update(List<GameObject> worldObjects, Size screenBounds)
        {
            // 1. קידום המונה
            cooldownCounter++;

            // 2. בדיקה האם הגענו לזמן הירי
            if (cooldownCounter >= fireRate)
            {
                // 3א. יצירת הקליע החדש. 
                // הוא נוצר באמצע הגובה של המלכודת כדי להיראות כאילו הוא יוצא מה"קנה"
                Projectile newProjectile = new Projectile(
                    this.X + (projectileSpeed > 0 ? this.Width : -10), // מיקום X לפי כיוון הירי
                    this.Y + (this.Height / 2) - 5,                    // מיקום Y ממורכז
                    10,                                                // רוחב הקליע
                    10,                                                // גובה הקליע
                    this.projectileSpeed                               // מהירות וכיוון
                );

                // 3ב. הוספת הקליע למשחק
                worldObjects.Add(newProjectile);

                // 3ג. איפוס המונה
                cooldownCounter = 0;
            }
        }

        public override void Draw(Graphics g)
        {
            // ציור גוף המלכודת
            g.FillRectangle(Brushes.DarkGray, X, Y, Width, Height);
            // ציור הקנה כדי שיהיה ברור לאן המלכודת מכוונת
            g.FillRectangle(Brushes.Black, X + (projectileSpeed > 0 ? Width : -10), Y + Height / 2 - 5, 10, 10);
        }
    }
}