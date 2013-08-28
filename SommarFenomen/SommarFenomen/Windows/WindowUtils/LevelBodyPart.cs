using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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
    }
}
