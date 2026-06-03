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
            // Cooldown counter
            cooldownCounter++;

            // If can fire
            if (cooldownCounter >= fireRate)
            {
                // Creating the projectile
                Projectile newProjectile = new Projectile(
                    this.X + (projectileSpeed > 0 ? this.Width : -10), // X position
                    this.Y + (this.Height / 2) - 5,                    // Centering Y position
                    10,                                                // Projectile Width
                    10,                                                // Projectile Height
                    this.projectileSpeed                               // Speed and direction
                );

                // Add the projectile to the game objects
                worldObjects.Add(newProjectile);

                // Cooldown counter reseting
                cooldownCounter = 0;
            }
        }

        public override void Draw(Graphics g)
        {
            // Paint the Trap
            g.FillRectangle(Brushes.DarkGray, X, Y, Width, Height);
            // Paint the nuzzle
            g.FillRectangle(Brushes.Black, X + (projectileSpeed > 0 ? Width : -10), Y + Height / 2 - 5, 10, 10);
        }
    }
}