using System.Security.Cryptography;
using UCH_Project.Classes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace UCH_Project
{
    public partial class Form1 : Form
    {
        public enum GameState { Menu, Building, Playing }
        private GameState currentState = GameState.Menu;

        private readonly int gridSize = 40; // Grid 40 x 40

        // Starter Width and Height for placement objects
        private int selectedWidth = 40;
        private int selectedHeight = 40;

        // Mouse coordinates
        private int previewX = -1;
        private int previewY = -1;

        private GameObject objectToPlace = null;

        private List<GameObject> gameObjects = new List<GameObject>();
        private List<GameObject> partyBoxOptions = new List<GameObject>();

        private Player player1;
        private Player player2;
        private System.Windows.Forms.Timer gameTimer;
        private Image cachedBackground;
        private Image cachedMenuDesign;
        private Image cachedPartyBox;
        private bool isPartyBoxOpen = false;
        private int itemsPlacedThisRound = 0;
        private bool isPlacingNewItem = false;
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
            StartButton.Click += BtnStart_Click;
            LoadButton.Click += BtnLoad_Click;
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 15;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
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


                // If one of the player's reached the end and won
                if (player1.HasWonRound || player2.HasWonRound)
                {
                    gameTimer.Stop();
                    string winner = player1.HasWonRound ? "1P [BLUE]" : "2P [PINK]";
                    MessageBox.Show($"{winner} Has reached the end and won the game!");

                    // Returning to the main menu
                    ReturnToMainMenu();
                    gameTimer.Start();
                    return;
                }


                //If both are dead
                if (player1.IsDead && player2.IsDead)
                {
                    gameTimer.Stop();
                    MessageBox.Show("Both players eliminated! - Restarting");
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

            // first layer - background image
            if (cachedBackground != null)
            {
                g.DrawImage(cachedBackground, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            // if in menu, we draw only the menu design.
            if (currentState == GameState.Menu)
            {
                if (cachedMenuDesign != null)
                {
                    g.DrawImage(cachedMenuDesign, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                }
                return;
            }

            // Second layer, the players and the game objects.
            if (currentState == GameState.Playing || currentState == GameState.Building)
            {
                foreach (GameObject obj in gameObjects)
                {
                    obj.Draw(g);
                }
            }

            // Third layer, party box and items
            if (currentState == GameState.Building)
            {
                if (isPartyBoxOpen)
                {
                    int boxY = 0;

                    if (cachedPartyBox != null)
                    {
                        int boxWidth = 953;
                        int boxHeight = 545;
                        int boxX = (this.ClientSize.Width - boxWidth) / 2;
                        boxY = (this.ClientSize.Height - boxHeight) / 2;

                        g.DrawImage(cachedPartyBox, boxX, boxY, boxWidth, boxHeight);
                    }

                    string turnText = itemsPlacedThisRound == 0 ? "Player 1's Turn [BLUE]" : "Player 2's Turn [PINK]";
                    Color textColor = itemsPlacedThisRound == 0 ? Color.DeepSkyBlue : Color.DeepPink;

                    // Players turn painting
                    using (Font turnFont = new Font("Impact", 36, FontStyle.Regular))
                    {
                        SizeF textSize = g.MeasureString(turnText, turnFont);
                        float textX = (this.ClientSize.Width - textSize.Width) / 2;
                        float textY = boxY - textSize.Height - 10;

                        using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                        {
                            g.DrawString(turnText, turnFont, shadowBrush, textX + 3, textY + 3);
                        }

                        using (SolidBrush turnBrush = new SolidBrush(textColor))
                        {
                            g.DrawString(turnText, turnFont, turnBrush, textX, textY);
                        }
                    }

                    // Paint for the party box option
                    foreach (GameObject option in partyBoxOptions)
                    {
                        option.Draw(g);
                    }
                }
                else
                {
                    // Make the building grid
                    using (Pen gridPen = new Pen(Color.FromArgb(40, Color.White), 1))
                    {
                        for (int x = 0; x < this.ClientSize.Width; x += gridSize)
                        {
                            g.DrawLine(gridPen, x, 0, x, this.ClientSize.Height);
                        }
                        for (int y = 0; y < this.ClientSize.Height; y += gridSize)
                        {
                            g.DrawLine(gridPen, 0, y, this.ClientSize.Width, y);
                        }
                    }

                    // the green / red outline that indicates where you can build
                    if (!string.IsNullOrEmpty(selectedObjectType) && previewX != -1)
                    {
                        bool isOccupied = IsGridSlotOccupied(previewX, previewY, selectedWidth, selectedHeight);
                        Color previewColor = isOccupied ? Color.FromArgb(100, Color.Red) : Color.FromArgb(100, Color.LimeGreen);

                        using (SolidBrush brush = new SolidBrush(previewColor))
                        {
                            g.FillRectangle(brush, previewX, previewY, selectedWidth, selectedHeight);
                        }

                        using (Pen borderPen = new Pen(isOccupied ? Color.Red : Color.LimeGreen, 2))
                        {
                            g.DrawRectangle(borderPen, previewX, previewY, selectedWidth, selectedHeight);
                        }
                    }
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //empty so it won't delete the current background
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            // Players controls - 1P - OnKeyDown
            if (e.KeyCode == Keys.Left) player1.SetMovingLeft(true);
            if (e.KeyCode == Keys.Right) player1.SetMovingRight(true);
            if (e.KeyCode == Keys.Up) player1.Jump();

            // 2P
            if (e.KeyCode == Keys.A) player2.SetMovingLeft(true);
            if (e.KeyCode == Keys.D) player2.SetMovingRight(true);
            if (e.KeyCode == Keys.W) player2.Jump();

            if (e.KeyCode == Keys.Enter && currentState == GameState.Building)
            {
                currentState = GameState.Playing;
                this.Invalidate();
            }

            // To save use CTRL+S
            if (e.KeyCode == Keys.S && e.Control)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Model Files (*.mdl)|*.mdl|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Save the current level";
                    saveFileDialog.DefaultExt = "mdl";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        LevelSerializer.SaveLevel(gameObjects, saveFileDialog.FileName);
                        MessageBox.Show("Level successfully saved!");
                    }
                }
            }

            // To load use CTRL + L
            if (e.KeyCode == Keys.L && e.Control)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Model Files (*.mdl)|*.mdl|All Files (*.*)|*.*";
                    openFileDialog.Title = "Load Save File";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        List<GameObject> loadedObjects = LevelSerializer.LoadLevel(openFileDialog.FileName);

                        if (loadedObjects != null)
                        {
                            gameObjects = loadedObjects;

                            List<Player> loadedPlayers = new List<Player>();
                            foreach (GameObject obj in gameObjects)
                            {
                                if (obj is Player p)
                                {
                                    loadedPlayers.Add(p);
                                }
                            }
                            if (loadedPlayers.Count >= 2)
                            {
                                player1 = loadedPlayers[0];
                                player2 = loadedPlayers[1];
                            }

                            MessageBox.Show("Save loaded successfully!");
                            this.Invalidate();
                        }
                    }
                }
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            // On KeyUp
            if (e.KeyCode == Keys.Left) player1.SetMovingLeft(false);
            if (e.KeyCode == Keys.Right) player1.SetMovingRight(false);
            if (e.KeyCode == Keys.Up) player1.WantsToJump = false;

            if (e.KeyCode == Keys.A) player2.SetMovingLeft(false);
            if (e.KeyCode == Keys.D) player2.SetMovingRight(false);
            if (e.KeyCode == Keys.W) player2.WantsToJump = false;
        }

        private void ReturnToMainMenu()
        {
            currentState = GameState.Menu;

            // Clearing the memory
            gameObjects.Clear();
            partyBoxOptions.Clear();

            // Returning everything back
            isPartyBoxOpen = false;
            isPlacingNewItem = false;
            selectedObjectType = "";
            previewX = -1;
            previewY = -1;

            StartButton.Visible = true;
            LoadButton.Visible = true;

            this.Invalidate();
        }

        private void ResetRoundForNextBuilding()
        {
            gameObjects.RemoveAll(obj => obj is Projectile);

            player1.X = 100;
            player1.Y = 475;
            player1.HasWonRound = false;
            player1.IsDead = false;
            player1.SetMovingLeft(false);
            player1.SetMovingRight(false);

            player2.X = 150;
            player2.Y = 475;
            player2.HasWonRound = false;
            player2.IsDead = false;
            player2.SetMovingLeft(false);
            player2.SetMovingRight(false);

            currentState = GameState.Building;

            itemsPlacedThisRound = 0;
            isPlacingNewItem = false;
            GeneratePartyBoxOptions();

            this.Focus();
            this.Invalidate();
        }

        private bool IsGridSlotOccupied(int x, int y, int width, int height)
        {
            // Creating a rectangle to see where you can build
            Rectangle newObjectRect = new Rectangle(x, y, width, height);

            foreach (GameObject obj in gameObjects)
            {
                if (obj is Player) continue;

                // Getting the real width and height
                int objW = 40;
                int objH = 40;

                if (obj is StaticObject sObj)
                {
                    objW = sObj.Width;
                    objH = sObj.Height;
                }
                else if (obj is ActionObject aObj)
                {
                    objW = aObj.Width;
                    objH = aObj.Height;
                }

                // Creating bounds of the existing object
                Rectangle existingObjBounds = new Rectangle(obj.X, obj.Y, objW, objH);

                // Is the new object intersect with an existing object
                if (newObjectRect.IntersectsWith(existingObjBounds))
                {
                    return true; // Space is taken
                }
            }
            return false; // Can place the object
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            // Build mode
            if (currentState == GameState.Building)
            {
                if (isPartyBoxOpen)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        // Deleting the items from the list
                        for (int i = 0; i < partyBoxOptions.Count; i++)
                        {
                            GameObject option = partyBoxOptions[i];
                            int optW = 40;
                            int optH = 40;

                            if (option is StaticObject sObj) { optW = sObj.Width; optH = sObj.Height; }
                            else if (option is ActionObject aObj) { optW = aObj.Width; optH = aObj.Height; }

                            Rectangle optionBounds = new Rectangle(option.X, option.Y, optW, optH);

                            if (optionBounds.Contains(e.Location))
                            {
                                if (option is Platform p)
                                {
                                    selectedObjectType = "Platform";
                                    selectedPlatformType = p.Type;

                                    if (p.Type == Platform.PlatformType.WoodHorizontal)
                                    {
                                        selectedWidth = 160;
                                        selectedHeight = 40;
                                    }
                                    else if (p.Type == Platform.PlatformType.WoodVertical)
                                    {
                                        selectedWidth = 40;
                                        selectedHeight = 160;
                                    }
                                    else // MetalBox
                                    {
                                        selectedWidth = 40;
                                        selectedHeight = 40;
                                    }
                                }
                                else if (option is MovingHazard)
                                {
                                    selectedObjectType = "MovingHazard";
                                    selectedWidth = 40;
                                    selectedHeight = 40;
                                }
                                else if (option is ProjectileTrap)
                                {
                                    selectedObjectType = "ProjectileTrap";
                                    selectedWidth = 40;
                                    selectedHeight = 40;
                                }

                                // Deleting the item from the party box for the other player to build
                                partyBoxOptions.RemoveAt(i);

                                isPartyBoxOpen = false; 
                                isPlacingNewItem = true;

                                this.Invalidate();
                                return;
                            }
                        }
                    }
                    return;
                }

                // Actual building on the map
                int snappedX = (e.X / gridSize) * gridSize;
                int snappedY = (e.Y / gridSize) * gridSize;

                // Right click - replace mode
                if (e.Button == MouseButtons.Right)
                {
                    Point clickPoint = e.Location;
                    GameObject objectToPickUp = null;

                    for (int i = gameObjects.Count - 1; i >= 0; i--)
                    {
                        GameObject obj = gameObjects[i];
                        if (obj is Player) continue;

                        int w = obj is StaticObject s ? s.Width : (obj is ActionObject a ? a.Width : 40);
                        int h = obj is StaticObject s2 ? s2.Height : (obj is ActionObject a2 ? a2.Height : 40);

                        Rectangle bounds = new Rectangle(obj.X, obj.Y, w, h);
                        if (bounds.Contains(clickPoint))
                        {
                            objectToPickUp = obj;
                            break;
                        }
                    }

                    if (objectToPickUp != null)
                    {
                        if (objectToPickUp is Platform p)
                        {
                            selectedObjectType = "Platform";
                            selectedPlatformType = p.Type;
                        }
                        else if (objectToPickUp is MovingHazard) { selectedObjectType = "MovingHazard"; }
                        else if (objectToPickUp is ProjectileTrap) { selectedObjectType = "ProjectileTrap"; }

                        selectedWidth = objectToPickUp is StaticObject st ? st.Width : (objectToPickUp is ActionObject ac ? ac.Width : 40);
                        selectedHeight = objectToPickUp is StaticObject st2 ? st2.Height : (objectToPickUp is ActionObject ac2 ? ac2.Height : 40);

                        gameObjects.Remove(objectToPickUp);
                        this.Invalidate();
                    }
                    return;
                }

                // Left click - place mode
                if (e.Button == MouseButtons.Left)
                {
                    if (IsGridSlotOccupied(snappedX, snappedY, selectedWidth, selectedHeight))
                    {
                        return;
                    }

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

                    // Check if placed
                    if (isPlacingNewItem)
                    {
                        itemsPlacedThisRound++;
                        isPlacingNewItem = false;

                        // If the first player has placed his item, then its time for the second player
                        if (itemsPlacedThisRound < 2)
                        {
                            isPartyBoxOpen = true;
                        }
                    }

                    this.Invalidate();
                }
            }
        }

        // Grid snap on building mode.
        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (currentState == GameState.Building && !string.IsNullOrEmpty(selectedObjectType))
            {
                int snappedX = (e.X / gridSize) * gridSize;
                int snappedY = (e.Y / gridSize) * gridSize;

                if (snappedX != previewX || snappedY != previewY)
                {
                    previewX = snappedX;
                    previewY = snappedY;
                    this.Invalidate();
                }
            }
            else if (previewX != -1)
            {
                previewX = -1;
                previewY = -1;
                this.Invalidate();
            }
        }

        // Party box random generation
        private void GeneratePartyBoxOptions()
        {
            partyBoxOptions.Clear();
            Random rand = new Random();

            int[] optionXs = { 625, 825, 1025 };
            int boxY = 320;

            for (int i = 0; i < 3; i++)
            {
                int choice = rand.Next(5);

                switch (choice)
                {
                    case 0:
                        partyBoxOptions.Add(new Platform(optionXs[i], boxY, 40, 40, Platform.PlatformType.MetalBox));
                        break;
                    case 1:
                        partyBoxOptions.Add(new Platform(optionXs[i] - 40, boxY + 20, 160, 40, Platform.PlatformType.WoodHorizontal));
                        break;
                    case 2:
                        partyBoxOptions.Add(new Platform(optionXs[i], boxY - 40, 40, 160, Platform.PlatformType.WoodVertical));
                        break;
                    case 3:
                        partyBoxOptions.Add(new ProjectileTrap(optionXs[i], boxY, 40, 40, 60, 7));
                        break;
                    case 4:
                        partyBoxOptions.Add(new MovingHazard(optionXs[i], boxY, 40, 40, 3));
                        break;
                }
            }

            isPartyBoxOpen = true;
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            gameObjects.Clear();

            player1 = new Player(100, 475, Brushes.Blue);
            player2 = new Player(150, 475, Brushes.DeepPink);

            gameObjects.Add(player1);
            gameObjects.Add(player2);

            // Setup starting ground
            Platform startingGround = new Platform(40, 520, 200, 40, Platform.PlatformType.MetalFloor);
            gameObjects.Add(startingGround);

            Random rand = new Random();

            int num = rand.Next(240, 440);
            int snappedY = (num / 40) * 40;

            gameObjects.Add(new Goal(1400, snappedY, 40, 80));

            currentState = GameState.Building;
            StartButton.Visible = false;
            LoadButton.Visible = false;

            itemsPlacedThisRound = 0;
            isPlacingNewItem = false;
            GeneratePartyBoxOptions(); // Generating party box options

            this.Focus();
            this.Invalidate();
        }

        private void BtnLoad_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Model Files (*.mdl)|*.mdl|All Files (*.*)|*.*";
                openFileDialog.Title = "Load Save File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    List<GameObject> loadedObjects = LevelSerializer.LoadLevel(openFileDialog.FileName);

                    if (loadedObjects != null)
                    {
                        gameObjects = loadedObjects;

                        List<Player> loadedPlayers = new List<Player>();
                        foreach (GameObject obj in gameObjects)
                        {
                            if (obj is Player p)
                            {
                                loadedPlayers.Add(p);
                            }
                        }

                        if (loadedPlayers.Count >= 2)
                        {
                            player1 = loadedPlayers[0];
                            player2 = loadedPlayers[1];
                        }

                        StartButton.Visible = false;
                        LoadButton.Visible = false;

                        currentState = GameState.Building;

                        itemsPlacedThisRound = 0;
                        isPlacingNewItem = false;
                        GeneratePartyBoxOptions();

                        this.Focus();
                        this.Invalidate();
                    }
                }
            }
        }
    }
}