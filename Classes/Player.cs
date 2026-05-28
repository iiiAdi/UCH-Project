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
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 50;

        private int moveSpeed = 6;
        private int jumpSpeed = 0;
        private int gravity = 2;

        public bool IsLeft { get; private set; }
        public bool IsRight { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsOnGround { get; set; }

        public Player(int startX, int startY)
        {
            this.X = startX;
            this.Y = startY;
        }
        
        public void SetMovingLeft(bool isMoving){
            IsLeft = isMoving;    
        }
        public void SetMovingRight(bool isMoving) {
            IsRight = isMoving;
        }

        public void Update()
        {
            if (IsLeft) X -= moveSpeed;
            if (IsRight) X += moveSpeed;

            Y += jumpSpeed;
            jumpSpeed += gravity;

            if (Y >= 400)
            {
                Y = 400;
                jumpSpeed = 0;
                IsOnGround = true;
                IsJumping = false;
            }
        }

        public void Jump()
        {
            if (IsOnGround)
            {
                jumpSpeed = -20;
                IsOnGround = false;
                IsJumping = true;
            }
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, X, Y, Width, Height);
        }
    }
}
