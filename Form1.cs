using UCH_Project.Classes;

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
        private bool isPlacingNewItem = false; // מבדיל בין בחירת חפץ חדש מהתיבה לבין הזזת חפץ קיים
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


                // בדיקת ניצחון: אם לפחות אחד מהשחקנים הגיע למטרה
                if (player1.HasWonRound || player2.HasWonRound)
                {
                    gameTimer.Stop();
                    string winner = player1.HasWonRound ? "שחקן 1 (כחול)" : "שחקן 2 (ורוד)";
                    MessageBox.Show($"כל הכבוד! {winner} הגיע למטרה והרוויח נקודה!");
                    ResetRoundForNextBuilding();
                    gameTimer.Start();
                    return;
                }

                // בדיקת הפסד: השלב נכשל רק אם *שני* השחקנים מתו
                if (player1.IsDead && player2.IsDead)
                {
                    gameTimer.Stop();
                    MessageBox.Show("אופס... שני השחקנים נפסלו! המלכודות ניצחו.");
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

            // 1. שכבה תחתונה: רקע המשחק
            if (cachedBackground != null)
            {
                g.DrawImage(cachedBackground, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            // אם אנחנו בתפריט, מציירים אותו ויוצאים (אין טעם לצייר את שאר העולם)
            if (currentState == GameState.Menu)
            {
                if (cachedMenuDesign != null)
                {
                    g.DrawImage(cachedMenuDesign, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                }
                return;
            }

            // 2. שכבה אמצעית: כל האובייקטים הקיימים במפה (שחקן, קוביות, מלכודות)
            // עכשיו הם יצויירו *לפני* התיבה, ולכן יהיו מתחתיה!
            if (currentState == GameState.Playing || currentState == GameState.Building)
            {
                foreach (GameObject obj in gameObjects)
                {
                    obj.Draw(g);
                }
            }

            // 3. שכבה עליונה: UI של שלב הבנייה (רשת, תצוגה מקדימה, ותיבת המסיבה)
            if (currentState == GameState.Building)
            {
                if (isPartyBoxOpen)
                {
                    // ציור תיבת המסיבה (מצוירת אחרונה = נמצאת מעל הכל!)
                    if (cachedPartyBox != null)
                    {
                        int boxWidth = 953;
                        int boxHeight = 545;
                        int boxX = (this.ClientSize.Width - boxWidth) / 2;
                        int boxY = (this.ClientSize.Height - boxHeight) / 2;

                        g.DrawImage(cachedPartyBox, boxX, boxY, boxWidth, boxHeight);
                    }

                    // ציור האפשרויות בתוך התיבה
                    foreach (GameObject option in partyBoxOptions)
                    {
                        option.Draw(g);
                    }
                }
                else
                {
                    // ציור רשת העזר לבנייה (Grid)
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

                    // ציור התצוגה המקדימה שעוקבת אחרי העכבר (ההצללית מהסעיף הקודם)
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
            // --- שחקן 1 (חצים) ---
            if (e.KeyCode == Keys.Left) player1.SetMovingLeft(true);
            if (e.KeyCode == Keys.Right) player1.SetMovingRight(true);
            if (e.KeyCode == Keys.Up) player1.Jump();

            // --- שחקן 2 (WASD) ---
            if (e.KeyCode == Keys.A) player2.SetMovingLeft(true);
            if (e.KeyCode == Keys.D) player2.SetMovingRight(true);
            if (e.KeyCode == Keys.W) player2.Jump();

            if (e.KeyCode == Keys.Enter && currentState == GameState.Building)
            {
                currentState = GameState.Playing;
                this.Invalidate();
            }

            // --- חלון שמירת משחק (Ctrl + S) ---
            if (e.KeyCode == Keys.S && e.Control)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Model Files (*.mdl)|*.mdl|All Files (*.*)|*.*";
                    saveFileDialog.Title = "בחר היכן לשמור את השלב";
                    saveFileDialog.DefaultExt = "mdl";

                    // אם המשתמש בחר נתיב ולחץ "שמור"
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        LevelSerializer.SaveLevel(gameObjects, saveFileDialog.FileName);
                        MessageBox.Show("השלב נשמר בהצלחה!");
                    }
                }
            }

            // --- חלון טעינת משחק (Ctrl + L) ---
            if (e.KeyCode == Keys.L && e.Control)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Model Files (*.mdl)|*.mdl|All Files (*.*)|*.*";
                    openFileDialog.Title = "בחר קובץ שלב לטעינה";

                    // אם המשתמש בחר קובץ ולחץ "פתח"
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        List<GameObject> loadedObjects = LevelSerializer.LoadLevel(openFileDialog.FileName);

                        if (loadedObjects != null)
                        {
                            gameObjects = loadedObjects;

                            // מתוך קטע הטעינה ב-Form1
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

                            MessageBox.Show("השלב נטען בהצלחה!");
                            this.Invalidate();
                        }
                    }
                }
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            // --- שחקן 1 ---
            if (e.KeyCode == Keys.Left) player1.SetMovingLeft(false);
            if (e.KeyCode == Keys.Right) player1.SetMovingRight(false);
            if (e.KeyCode == Keys.Up) player1.WantsToJump = false;

            // --- שחקן 2 ---
            if (e.KeyCode == Keys.A) player2.SetMovingLeft(false);
            if (e.KeyCode == Keys.D) player2.SetMovingRight(false);
            if (e.KeyCode == Keys.W) player2.WantsToJump = false;
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
            // יצירת מלבן שמייצג את המיקום והגודל המלא של האובייקט החדש שרוצים לבנות
            Rectangle newObjectRect = new Rectangle(x, y, width, height);

            foreach (GameObject obj in gameObjects)
            {
                if (obj is Player) continue;

                // שולפים את המידות האמיתיות של האובייקט הקיים במפה
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

                // יצירת מלבן הגבולות של האובייקט הקיים
                Rectangle existingObjBounds = new Rectangle(obj.X, obj.Y, objW, objH);

                // בדיקה: האם המלבן של החפץ החדש מתנגש/חופף במקום כלשהו עם החפץ הקיים?
                if (newObjectRect.IntersectsWith(existingObjBounds))
                {
                    return true; // השטח תפוס (באחד החלקים של האובייקט החדש או הקיים)!
                }
            }
            return false; // השטח פנוי לחלוטין לכל אורך ורוחב האובייקט
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (currentState == GameState.Building)
            {
                // ==========================================
                // שלב א': בחירת חפץ מה-Party Box
                // ==========================================
                if (isPartyBoxOpen)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        // שימוש בלולאת for מאפשר לנו למחוק איברים מהרשימה בבטחה
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

                                // מסירים את החפץ מהתיבה כדי שהשחקן השני יבחר ממה שנשאר
                                partyBoxOptions.RemoveAt(i);

                                isPartyBoxOpen = false; // סוגרים את התיבה זמנית כדי למקם את החפץ
                                isPlacingNewItem = true; // מסמנים שלקחנו חפץ חדש

                                this.Invalidate();
                                return;
                            }
                        }
                    }
                    return; // התעלמות מלחיצות על הרקע של התיבה
                }

                // ==========================================
                // שלב ב': בנייה או מחיקה על המפה עצמה
                // ==========================================

                int snappedX = (e.X / gridSize) * gridSize;
                int snappedY = (e.Y / gridSize) * gridSize;

                // --- לוגיקת כפתור ימני (הזזת חפץ קיים) ---
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

                // --- לוגיקת כפתור שמאלי (הנחת חפץ) ---
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

                    // בדיקה אם הרגע הנחנו חפץ חדש שהוצאנו מהתיבה
                    if (isPlacingNewItem)
                    {
                        itemsPlacedThisRound++;
                        isPlacingNewItem = false;

                        // אם זה השחקן הראשון שהניח, נפתח את התיבה מחדש לשחקן השני!
                        if (itemsPlacedThisRound < 2)
                        {
                            isPartyBoxOpen = true;
                        }
                    }

                    this.Invalidate();
                }
            }
        }

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

        private void GeneratePartyBoxOptions()
        {
            partyBoxOptions.Clear();
            Random rand = new Random();

            int[] optionXs = { 625, 825, 1025 };
            int boxY = 320;

            for (int i = 0; i < 3; i++)
            {
                // שינוי ל-5 כדי לכלול את כל האפשרויות
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
                    case 4: // הוספת המכשול הנע שיצרנו
                        partyBoxOptions.Add(new MovingHazard(optionXs[i], boxY, 40, 40, 3));
                        break;
                }
            }

            isPartyBoxOpen = true;
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            gameObjects.Clear();

            // שחקן 1 (כחול) - ישחק עם החצים
            player1 = new Player(100, 475, Brushes.Blue);
            // שחקן 2 (ורוד) - ישחק עם WASD
            player2 = new Player(150, 475, Brushes.DeepPink);

            gameObjects.Add(player1);
            gameObjects.Add(player2);

            // Setup starting ground
            Platform startingGround = new Platform(40, 520, 200, 40, Platform.PlatformType.MetalFloor);
            gameObjects.Add(startingGround);

            gameObjects.Add(new Goal(1400, 500, 40, 80));

            currentState = GameState.Building;
            StartButton.Visible = false;

            itemsPlacedThisRound = 0;
            isPlacingNewItem = false;
            GeneratePartyBoxOptions(); // Generating party box options

            this.Focus();
            this.Invalidate();
        }
    }
}