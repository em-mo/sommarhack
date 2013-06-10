using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SommarFenomen
{

    class WindowHandler
    {
        private Window currentWindow;

        public void UpdateWindow(GameTime gameTime)
        {
            currentWindow.Update(gameTime);
        }

        public void DrawWindowGraphics(GameTime gameTime)
        {
            currentWindow.Draw(gameTime);
        }
        
        public void ChangeWindow(Window newWindow)
        {
            currentWindow = newWindow;
        }
    }
}
