using UCH_Project.Classes;

namespace UCH_Project
{
    public partial class Form1 : Form
    {
        public enum GameState { Menu, Building, Playing }
        private GameState currentState = GameState.Menu;

        private readonly int gridSize = 40; // Grid 40 x 40
        
        private int selectedWidth = 40;
        private int selectedHeight = 40;

        private GameObject objectToPlace = null;

        private List<GameObject> gameObjects = new List<GameObject>();
        private List<GameObject> partyBoxOptions = new List<GameObject>();

        private Player player;
        private System.Windows.Forms.Timer gameTimer;
        private Image cachedBackground;
        private Image cachedMenuDesign;
        private Image cachedPartyBox;
        private bool isPartyBoxOpen = false;

        private string selectedObjectType = "";
        private Platform.PlatformType selectedPlatformType;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.DoubleBuffered = true;

            cachedBackground = Properties.Resources.fitBackground;
            cachedMenuDesign = Properties.Resources.Menu;
            cachedPartyBox = Properties.Resources.PartyBox;

            player = new Player(100, 100);

            StartButton.Click += BtnStart_Click;

            gameObjects.Add(player);

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 15;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.MouseDown += Form1_MouseDown;

            Platform woodFloor = new Platform(50, 600, 196, 26, Platform.PlatformType.WoodHorizontal);
            gameObjects.Add(woodFloor);

            Platform woodWall = new Platform(400, 300, 26, 191, Platform.PlatformType.WoodVertical);
            gameObjects.Add(woodWall);

            Platform metalBlock = new Platform(600, 300, 50, 50, Platform.PlatformType.MetalBox);
            gameObjects.Add(metalBlock);

            // creating hazard
            MovingHazard movingSaw = new MovingHazard(400, 400, 40, 40, 5);

            // adding the hazard to the gameObjects
            gameObjects.Add(movingSaw);

            Goal endGoal = new Goal(700, 250, 40, 40);

            // Adding the end goal to the gameObjects
            gameObjects.Add(endGoal);

            // Add Trap
            ProjectileTrap trap = new ProjectileTrap(50, 200, 40, 40, 60, 7);
            gameObjects.Add(trap);
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (currentState == GameState.Playing)
            {
               
                for (int i = gameObjects.Count - 1; i >= 0; i--)
                {
                    gameObjects[i].Update(gameObjects, this.ClientSize);
                }
                gameObjects.RemoveAll(obj => obj.IsDestroyed);

               
                if (player.HasWonRound)
                {
                    gameTimer.Stop();

                    MessageBox.Show("כל הכבוד! הגעת למטרה והרווחת נקודה!");
                    ResetRoundForNextBuilding();

                    gameTimer.Start();
                    return; 
                }
              
                else if (player.IsDead)
                {
                    gameTimer.Stop();

                    MessageBox.Show("אופס... המלכודות ניצחו הפעם!");
                    ResetRoundForNextBuilding();

                    gameTimer.Start();
                    return; 
                }
            }

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

            if (currentState == GameState.Building)
            {
                if (isPartyBoxOpen)
                {
                    // Paint the party box
                    if (cachedPartyBox != null)
                    {
                        int boxWidth = 953;
                        int boxHeight = 545;
                        int boxX = (this.ClientSize.Width - boxWidth) / 2;
                        int boxY = (this.ClientSize.Height - boxHeight) / 2;

                        g.DrawImage(cachedPartyBox, boxX, boxY, boxWidth, boxHeight);
                    }

                    // Paint the three objects that were picked in the RNG
                    foreach (GameObject option in partyBoxOptions)
                    {
                        option.Draw(g);
                    }
                }
                else
                {
                    // Grid for building
                    using (Pen gridPen = new Pen(Color.FromArgb(50, Color.White), 1))
                    {
                        for (int x = 0; x < this.ClientSize.Width; x += gridSize) g.DrawLine(gridPen, x, 0, x, this.ClientSize.Height);
                        for (int y = 0; y < this.ClientSize.Height; y += gridSize) g.DrawLine(gridPen, 0, y, this.ClientSize.Width, y);
                    }
                }
            }

            if (currentState == GameState.Menu)
            {
                if (cachedMenuDesign != null)
                {
                    g.DrawImage(cachedMenuDesign, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                }
            }
            else if (currentState == GameState.Playing || currentState == GameState.Building)
            {
                foreach (GameObject obj in gameObjects)
                {
                    obj.Draw(g);
                }
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
            if (e.KeyCode == Keys.Enter && currentState == GameState.Building)
            {
                currentState = GameState.Playing;
                this.Invalidate();
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) player.SetMovingLeft(false);
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) player.SetMovingRight(false);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.W) player.WantsToJump = false;
        }


        private void ResetRoundForNextBuilding()
        {
            player.X = 100;
            player.Y = 500;
            player.HasWonRound = false;
            player.IsDead = false;
            player.SetMovingLeft(false);
            player.SetMovingRight(false);

            currentState = GameState.Building;

            GeneratePartyBoxOptions();

            this.Focus();
            this.Invalidate();
        }

        private bool IsGridSlotOccupied(int x, int y)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj is Player) continue;

                if (obj.X == x && obj.Y == y)
                {
                    return true;
                }
            }
            return false;
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (currentState == GameState.Building)
            {
                // Select from party box
                if (isPartyBoxOpen)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        foreach (GameObject option in partyBoxOptions)
                        {
                            int optW = gridSize;
                            int optH = gridSize;

                            if (option is StaticObject sObj)
                            {
                                optW = sObj.Width;
                                optH = sObj.Height;
                            }
                            else if (option is ActionObject aObj)
                            {
                                optW = aObj.SpeedX;
                                optW = aObj.Width;
                                optH = aObj.Height;
                            }


                            Rectangle optionBounds = new Rectangle(option.X, option.Y, optW, optH);

                            if (optionBounds.Contains(e.Location))
                            {
                                selectedWidth = optW;
                                selectedHeight = optH;

                                if (option is Platform p)
                                {
                                    selectedObjectType = "Platform";
                                    selectedPlatformType = p.Type;
                                }
                                else if (option is MovingHazard) { selectedObjectType = "MovingHazard"; }
                                else if (option is ProjectileTrap) { selectedObjectType = "ProjectileTrap"; }

                                isPartyBoxOpen = false;
                                this.Invalidate();
                                return;
                            }
                        }
                    }
                    return;
                }

                int snappedX = (e.X / gridSize) * gridSize;
                int snappedY = (e.Y / gridSize) * gridSize;

                if (e.Button == MouseButtons.Right)
                {
                    gameObjects.RemoveAll(obj => !(obj is Player) && obj.X == snappedX && obj.Y == snappedY);
                    this.Invalidate();
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    if (IsGridSlotOccupied(snappedX, snappedY)) return;

                    switch (selectedObjectType)
                    {
                        case "Platform":
                            gameObjects.Add(new Platform(snappedX, snappedY, selectedWidth, selectedHeight, selectedPlatformType));
                            break;

                        case "MovingHazard":
                            gameObjects.Add(new MovingHazard(snappedX, snappedY, selectedWidth, selectedHeight, 3));
                            break;

                        case "ProjectileTrap":
                            gameObjects.Add(new ProjectileTrap(snappedX, snappedY, selectedWidth, selectedHeight, 60, 7));
                            break;
                    }

                    selectedObjectType = "";
                    this.Invalidate();
                }
            }
        }

        private void GeneratePartyBoxOptions()
        {
            partyBoxOptions.Clear();
            Random rand = new Random();

            int[] optionXs = { 625, 850, 985 };
            int boxY = 300;

            for (int i = 0; i < 3; i++)
            {
                int choice = rand.Next(4);

                switch (choice)
                {
                    case 0:
                        partyBoxOptions.Add(new Platform(optionXs[i], boxY + 40, gridSize, gridSize, Platform.PlatformType.MetalBox));
                        break;

                    case 1:
                        partyBoxOptions.Add(new Platform(optionXs[i] - 40, boxY + 40, gridSize * 4, gridSize, Platform.PlatformType.WoodHorizontal));
                        break;

                    case 2: 
                        partyBoxOptions.Add(new Platform(optionXs[i], boxY, gridSize, gridSize * 4, Platform.PlatformType.WoodVertical));
                        break;

                    case 3:
                        partyBoxOptions.Add(new ProjectileTrap(optionXs[i], boxY + 40, gridSize, gridSize, 60, 7));
                        break;
                }
            }

            isPartyBoxOpen = true; // Opening the Party Box
        }


        private void BtnStart_Click(object? sender, EventArgs e)
        {
            gameObjects.Clear();

            player = new Player(100, 500);
            gameObjects.Add(player);
            gameObjects.Add(new Goal(1400, 500, 40, 80));

            currentState = GameState.Building;
            StartButton.Visible = false;

            GeneratePartyBoxOptions(); // Generating party box options

            this.Focus();
            this.Invalidate();
        }
    }
}