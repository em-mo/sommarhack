using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Util;

namespace SommarFenomen.Windows
{
    class LevelSelectWindow : Window
    {
        private WindowHandler _windowHandler;
        private SpriteBatch _spriteBatch;
        private Camera2D _camera;

        public LevelSelectWindow(WindowHandler windowHandler)
        {
            _windowHandler = windowHandler;
            GraphicsDevice graphicsDevice = _windowHandler.Game.GraphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _camera = new Camera2D(graphicsDevice);
        }

        public void Initialize()
        {
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void OnChange()
        {
            throw new NotImplementedException();
        }
    }
}
