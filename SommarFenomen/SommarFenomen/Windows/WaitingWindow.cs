using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SommarFenomen.Windows
{
    class WaitingWindow : Window
    {
        private KinectHandler _kinectHandler;
        private WindowHandler _windowHandler;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private string _waitString;

        public WaitingWindow(WindowHandler windowHandler)
        {
            _windowHandler = windowHandler;
            _kinectHandler = _windowHandler.Game.KinectHandler;
            _spriteBatch = new SpriteBatch(_windowHandler.Game.GraphicsDevice);
            _spriteFont = Game1.contentManager.Load<SpriteFont>(@"MyFont");
            startTimer = 0;
            _waitString = "Waiting for player";
        }

        public void Initialize()
        {
            startTimer = 0;
        }

        private static readonly double START_TIME = 5;
        private double startTimer;
        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (_kinectHandler.HasSkeleton())
            {
                startTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (startTimer > START_TIME)
                {
                    _windowHandler.ChangeWindow(_windowHandler.PlayWindow);
                }
            }
            else
            {
                startTimer = 0;
            }
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _spriteBatch.GraphicsDevice.Clear(Color.RosyBrown);
            _spriteBatch.Begin();

            Vector2 position, stringDimensions;
            stringDimensions = _spriteFont.MeasureString(_waitString);

            position.X = (_spriteBatch.GraphicsDevice.Viewport.Width - stringDimensions.X) / 2;
            position.Y = (_spriteBatch.GraphicsDevice.Viewport.Height - stringDimensions.Y) / 2;


            _spriteBatch.DrawString(_spriteFont, _waitString, position, Color.BlueViolet);
            

            _spriteBatch.End();
        }

        public void OnChange()
        {
            startTimer = 0;
        }
    }
}
