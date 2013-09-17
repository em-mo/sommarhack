using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using SommarFenomen.Util;

namespace SommarFenomen.Windows.WindowUtils
{
    class LevelBodyPart
    {
        public BodyPartType Position { get; set; }
        public List<string> LevelFiles { get; set; }
        public bool Dead = false;
        public int LevelNumber { get; set; }

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
            string levelDir = "levels/";
            DirectoryInfo dir = new DirectoryInfo(levelDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException();

            FileInfo[] files = dir.GetFiles("*.map");
            foreach (FileInfo file in files)
            {
                LevelBodyPart bodyPart = new LevelBodyPart();
                
                using (StreamReader readFileStream = new StreamReader(file.FullName))
                {
                    string line = readFileStream.ReadLine();
                    string[] words = line.Split(' ');

                    for (int i = 0; i < words.Count(); i++)
			        {
                        if (words[i] == "Position")
                        {
                            i++;
                            foreach (var part in Enum.GetValues(typeof(BodyPartType)))
                            {
                                if (Enum.GetName(typeof(BodyPartType), part).Equals(words[i], StringComparison.CurrentCultureIgnoreCase))
                                {
                                    bodyPart.Position = (BodyPartType)part;
                                    break;
                                }
                            }

                        }
                        else if (words[i] == "LevelNumber")
                        {
                            bodyPart.LevelNumber = int.Parse(words[++i]);
                        }
                    }
                }

                FileInfo[] mapFiles = dir.GetFiles(Path.GetFileNameWithoutExtension(file.Name) + '*' + ".png");
                foreach (var map in mapFiles)
                {
                    bodyPart.AddLevel(map.FullName);
                }
                parts.Add(bodyPart);
            }

            return parts;
        }
    }
}
