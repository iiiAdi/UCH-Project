using System;
using System.Collections.Generic;
using System.Drawing;

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
        public bool HasWonRound { get; set; } = false;
        public bool IsDead { get; set; } = false;
        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);
        public bool WantsToJump { get; set; } = false;
        public Brush PlayerColor { get; set; }
        public Player(int startX, int startY, Brush color)
        {
            this.X = startX;
            this.Y = startY;
            this.PlayerColor = color;
        }

        public void Jump()
        {
            WantsToJump = true;
        }

        public void SetMovingLeft(bool isMoving) => IsLeft = isMoving;
        public void SetMovingRight(bool isMoving) => IsRight = isMoving;

        // Updates the movement of the player
        public override void Update(List<GameObject> worldObjects, Size screenBounds)
        {
            // אם השחקן מת או ניצח, הוא מפסיק להיות מושפע מהפיזיקה ומהעולם
            if (IsDead || HasWonRound) return;

            if (IsLeft) X -= moveSpeed;
            if (IsRight) X += moveSpeed;

            CheckHorizontalCollisions(worldObjects);

            if (X < 0) X = 0;
            if (X > screenBounds.Width - this.Width) X = screenBounds.Width - this.Width;

            Y += jumpSpeed;
            jumpSpeed += gravity;

            CheckVerticalCollisions(worldObjects);
            CheckHazardCollisions(worldObjects, screenBounds);

            if (Y > screenBounds.Height)
            {
                IsDead = true;
            }

            if (WantsToJump && IsOnGround)
            {
                jumpSpeed = -20;
                IsOnGround = false;
                IsJumping = true;
            }

            WantsToJump = false;

            CheckGoalCollision(worldObjects, screenBounds);
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


        private void CheckHazardCollisions(List<GameObject> worldObjects, Size screenBounds)
        {
            foreach (GameObject obj in worldObjects)
            {
                if (obj is ActionObject hazard)
                {
                    if (this.Bounds.IntersectsWith(hazard.Bounds))
                    {
                        IsDead = true;
                        return;
                    }
                }
            }
        }

        private void CheckGoalCollision(List<GameObject> worldObjects, Size screenBounds)
        {
            foreach (GameObject obj in worldObjects)
            {
                if (obj is Goal goal)
                {
                    if (this.Bounds.IntersectsWith(goal.Bounds))
                    {
                        HasWonRound = true;
                        return;
                    }
                }
            }
        }

        public override void Draw(Graphics g)
        {
            // שחקן שנפסל לא יצויר על המסך
            if (IsDead) return;

            // ציור השחקן לפי הצבע הייחודי שלו
            g.FillRectangle(PlayerColor, X, Y, Width, Height);
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