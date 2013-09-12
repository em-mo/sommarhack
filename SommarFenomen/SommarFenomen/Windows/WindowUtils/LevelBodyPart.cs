using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace SommarFenomen.Windows.WindowUtils
{
    class LevelBodyPart
    {
        public Vector2 Position { get; set; }
        public List<string> LevelFiles { get; set; }
        public bool Dead = false;

        public LevelBodyPart()
        {
            LevelFiles = new List<string>();
        }

        public void AddLevel(string level)
        {
            LevelFiles.Add(level);
        }

        public static List<LevelBodyPart> LoadAllParts()
        {
            List<LevelBodyPart> parts = new List<LevelBodyPart>();
            string levelDir = "/levels/";
            DirectoryInfo dir = new DirectoryInfo(levelDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException();

            FileInfo[] files = dir.GetFiles("*.map");
            foreach (FileInfo file in files)
            {
                LevelBodyPart bodyPart = new LevelBodyPart();
                string pngName = Path.GetFileNameWithoutExtension(file.Name) + ".png";
                
                using (StreamReader readFileStream = new StreamReader(file.FullName))
                {
                    string line = readFileStream.ReadLine();
                    string[] words = line.Split(' ');

                    for (int i = 0; i < words.Count(); i++)
			        {
                        if (words[i] == "Position")
                        {
                            int x = int.Parse(words[++i]);
                            int y = int.Parse(words[++i]);
                            bodyPart.Position = new Vector2(x, y);
                        }
                        else if (words[i] == "Level")
                        {

                        }
                    }
                }

                FileInfo[] mapFiles = dir.GetFiles(Path.GetFileNameWithoutExtension(file.Name) + '*' + ".png");
                foreach (var map in mapFiles)
                {
                    bodyPart.AddLevel(map.FullName);
                }
            }

            return parts;
        }
    }
}
