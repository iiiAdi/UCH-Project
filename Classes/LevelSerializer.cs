using System;
using System.Collections.Generic;
using System.IO;

namespace UCH_Project.Classes
{
    public static class LevelSerializer
    {
        // Save the current level and positions of current objects
        public static void SaveLevel(List<GameObject> gameObjects, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                foreach (GameObject obj in gameObjects)
                {
                    if (obj is Player p)
                    {
                        sw.WriteLine($"Player,{p.X},{p.Y}");
                    }
                    else if (obj is Platform pl)
                    {
                        sw.WriteLine($"Platform,{pl.X},{pl.Y},{pl.Width},{pl.Height},{pl.Type}");
                    }
                    else if (obj is MovingHazard mh)
                    {
                        sw.WriteLine($"MovingHazard,{mh.X},{mh.Y},{mh.Width},{mh.Height},{mh.SpeedX}");
                    }
                    else if (obj is ProjectileTrap pt)
                    {
                        sw.WriteLine($"ProjectileTrap,{pt.X},{pt.Y},{pt.Width},{pt.Height}");
                    }
                    else if (obj is Goal g)
                    {
                        sw.WriteLine($"Goal,{g.X},{g.Y},{g.Width},{g.Height}");
                    }
                }
            }
        }

           
        // Load an existing save
        public static List<GameObject> LoadLevel(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            List<GameObject> loadedObjects = new List<GameObject>();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(',');
                string type = parts[0];
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);

                switch (type)
                {
                    case "Player":
                        bool isFirstPlayer = loadedObjects.FindAll(o => o is Player).Count == 0;
                        Brush pColor = isFirstPlayer ? Brushes.Blue : Brushes.DeepPink;

                        loadedObjects.Add(new Player(x, y, pColor));
                        break;
                    case "Platform":
                        int pWidth = int.Parse(parts[3]);
                        int pHeight = int.Parse(parts[4]);
                        Platform.PlatformType pType = (Platform.PlatformType)Enum.Parse(typeof(Platform.PlatformType), parts[5]);
                        loadedObjects.Add(new Platform(x, y, pWidth, pHeight, pType));
                        break;
                    case "MovingHazard":
                        int mWidth = int.Parse(parts[3]);
                        int mHeight = int.Parse(parts[4]);
                        int speedX = int.Parse(parts[5]);
                        loadedObjects.Add(new MovingHazard(x, y, mWidth, mHeight, speedX));
                        break;
                    case "ProjectileTrap":
                        int tWidth = int.Parse(parts[3]);
                        int tHeight = int.Parse(parts[4]);
                        loadedObjects.Add(new ProjectileTrap(x, y, tWidth, tHeight, 60, 7));
                        break;
                    case "Goal":
                        int gWidth = int.Parse(parts[3]);
                        int gHeight = int.Parse(parts[4]);
                        loadedObjects.Add(new Goal(x, y, gWidth, gHeight));
                        break;
                }
            }

            return loadedObjects;
        }
    }
}