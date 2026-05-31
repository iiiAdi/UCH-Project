using UCH_Project.Classes;

namespace UCH_Project
{
    public partial class Form1 : Form
    {
        private List<GameObject> gameObjects = new List<GameObject>();
        private Player player;
        private System.Windows.Forms.Timer gameTimer;
        private Image cachedBackground;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.DoubleBuffered = true;

            cachedBackground = Properties.Resources.fitBackground;

            player = new Player(100, 100);
            gameObjects.Add(player);

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 15;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            Platform woodFloor = new Platform(50, 600, 196, 26, Platform.PlatformType.WoodHorizontal);
            gameObjects.Add(woodFloor);

            Platform woodWall = new Platform(400, 300, 26, 191, Platform.PlatformType.WoodVertical);
            gameObjects.Add(woodWall);

            Platform metalBlock = new Platform(600, 300, 50, 50, Platform.PlatformType.MetalBox);
            gameObjects.Add(metalBlock);
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            player.Update();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if (cachedBackground != null)
            {
                g.DrawImage(cachedBackground, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            foreach (GameObject obj in gameObjects)
            {
                obj.Draw(g);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //empty so it won't delete the current background
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) player.SetMovingLeft(true);
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) player.SetMovingRight(true);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.W) player.Jump();
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) player.SetMovingLeft(false);
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) player.SetMovingRight(false);
        }

    }
}