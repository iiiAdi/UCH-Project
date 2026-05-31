using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;


namespace UCH_Project.Classes
{
    public class Player : GameObject
    {
        public int Width { get; set; } = 40;
        public int Height { get; set; } = 50;

        private int moveSpeed = 6;
        private int jumpSpeed = 0;
        private int gravity = 2;

        public bool IsLeft { get; private set; }
        public bool IsRight { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsOnGround { get; set; }
        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);
        public bool WantsToJump { get; set; } = false;

        public Player(int startX, int startY)
        {
            this.X = startX;
            this.Y = startY;
        }

        public void Jump()
        {
            WantsToJump = true;
        }

        public void SetMovingLeft(bool isMoving) => IsLeft = isMoving;
        public void SetMovingRight(bool isMoving) => IsRight = isMoving;

        // Updates the movement of the player
        public void Update(List<GameObject> worldObjects, Size screenBounds)
        {
            if (IsLeft) X -= moveSpeed;
            if (IsRight) X += moveSpeed;

            CheckHorizontalCollisions(worldObjects);

            if (X < 0)
            {
                X = 0;
            }
            if (X > screenBounds.Width - this.Width)
            {
                X = screenBounds.Width - this.Width;
            }

            Y += jumpSpeed;
            jumpSpeed += gravity;

            CheckVerticalCollisions(worldObjects);

            if (Y > screenBounds.Height)
            {
                Respawn(screenBounds);
            }

            if (WantsToJump && IsOnGround)
            {
                jumpSpeed = -20;
                IsOnGround = false;
                IsJumping = true;
            }

            WantsToJump = false;
        }

        private void CheckHorizontalCollisions(List<GameObject> worldObjects)
        {
            foreach (GameObject obj in worldObjects)
            {
                if (obj is StaticObject staticObj && staticObj.IsCollidable)
                {
                    if (this.Bounds.IntersectsWith(staticObj.Bounds))
                    {
                        if (IsRight)
                        {
                            X = staticObj.X - this.Width;
                        }
                        else if (IsLeft)
                        {
                            X = staticObj.X + staticObj.Width;
                        }
                    }
                }
            }
        }

        private void CheckVerticalCollisions(List<GameObject> worldObjects)
        {
            IsOnGround = false;

            foreach (GameObject obj in worldObjects)
            {
                if (obj is StaticObject staticObj && staticObj.IsCollidable)
                {
                    if (this.Bounds.IntersectsWith(staticObj.Bounds))
                    {
                        if (jumpSpeed > 0)
                        {
                            Y = staticObj.Y - this.Height;
                            jumpSpeed = 1;
                            IsOnGround = true;
                            IsJumping = false;
                        }
                        else if (jumpSpeed < 0)
                        {
                            Y = staticObj.Y + staticObj.Height;
                            jumpSpeed = 0;
                        }
                    }
                }
            }
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, X, Y, Width, Height);
        }

        private void Respawn(Size screenBounds)
        {
            X = 100;
            Y = 100;
            jumpSpeed = 0;
            IsOnGround = false;
            IsJumping = false;
        }
    }
}
