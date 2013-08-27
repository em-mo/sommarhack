using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SommarFenomen.Windows;

namespace SommarFenomen
{

    class WindowHandler
    {
        public PlayWindow PlayWindow { get; set; }
        public WaitingWindow WaitingWindow { get; set; }
        public LevelSelectWindow LevelSelectWindow { get; set; }

        private Window _currentWindow;
        public Game1 Game { get; set; }

        public WindowHandler(Game1 game)
        {
            Game = game;
        }

        public void UpdateWindow(GameTime gameTime)
        {
            _currentWindow.Update(gameTime);
        }

        public void DrawWindowGraphics(GameTime gameTime)
        {
            _currentWindow.Draw(gameTime);
        }
        
        public void ChangeWindow(Window newWindow)
        {
            _currentWindow = newWindow;
        }
    }
}
