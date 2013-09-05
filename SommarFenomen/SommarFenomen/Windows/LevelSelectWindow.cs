using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Util;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using SommarFenomen.Windows.WindowUtils;

namespace SommarFenomen.Windows
{
    class LevelSelectWindow : Window
    {
        private WindowHandler _windowHandler;
        private SpriteBatch _spriteBatch;
        private Camera2D _camera;

        private List<Sprite> _spriteList;
        private Sprite _background;
        private Sprite _dot;

        private Stopwatch _bodyPartPickTimer = new Stopwatch();
        private Stopwatch _zoomTimer = new Stopwatch();
        private Stopwatch _startGameTimer = new Stopwatch();
        private List<LevelBodyPart> _bodyParts = new List<LevelBodyPart>();
        private LevelBodyPart _chosenBodyPart;
        private static readonly float START_ZOOM = 0.25F;
        private static readonly float ZOOM_FACTOR = 1.02f;
        private static readonly double PICK_TIME = 1;
        private static readonly double ZOOM_TIME = 1;
        private static readonly double START_GAME_TIME = 3;

        public LevelSelectWindow(WindowHandler windowHandler)
        {
            _windowHandler = windowHandler;
            GraphicsDevice graphicsDevice = _windowHandler.Game.GraphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _camera = new Camera2D(graphicsDevice);
            
            _spriteList = new List<Sprite>();

            _background = new Sprite(Game1.contentManager.Load<Texture2D>(@"Images\LevelSelectMan"));
            float windowScaleRatio = graphicsDevice.Viewport.Height / _background.OriginalSize.Y;
            _background.Scale = new Vector2(windowScaleRatio / START_ZOOM);
            _background.CenterOrigin();

            _background.Position = Utils.GetScreenCenter(graphicsDevice);

            _dot = new Sprite(Game1.contentManager.Load<Texture2D>(@"Images\Dot"));
            _dot.CenterOrigin();
            _dot.Color = Color.DarkRed;

            _spriteList.Add(_background);
        }

        public void Initialize()
        {
            LevelBodyPart part = new LevelBodyPart();
            Vector2 position;
            position.X = _spriteBatch.GraphicsDevice.Viewport.Width / 2;
            position.Y = _spriteBatch.GraphicsDevice.Viewport.Height / 2;
            part.Position = position;
            part.LevelFiles.Add(@"levels\test");
            _bodyParts.Add(part);
        }

        private LevelBodyPart ChooseHealthyBodyPart()
        {
            List<LevelBodyPart> healthyBodyParts = new List<LevelBodyPart>();
            foreach (var bodyPart in _bodyParts)
            {
                if (!bodyPart.Dead)
                    healthyBodyParts.Add(bodyPart);
            }

            if (healthyBodyParts.Count != 0)
                return healthyBodyParts[Shared.Random.Next(healthyBodyParts.Count)];

                //Bob's dead!
            else
                return _bodyParts[Shared.Random.Next(_bodyParts.Count)];
        }

        private void StartGame()
        {
            string level = _chosenBodyPart.LevelFiles[Shared.Random.Next(_chosenBodyPart.LevelFiles.Count)];
            _windowHandler.ChangeWindow(_windowHandler.PlayWindow, level);
        }


        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (_bodyPartPickTimer.Elapsed.TotalSeconds > PICK_TIME)
            {
                _bodyPartPickTimer.Reset();
                _zoomTimer.Start();
                _dot.Position = _chosenBodyPart.Position;
                _spriteList.Add(_dot);
            }
            else if (_zoomTimer.Elapsed.TotalSeconds > ZOOM_TIME)
            {
                _camera.Position = _chosenBodyPart.Position;
                _camera.Zoom *= ZOOM_FACTOR;
            }
            
            if (_startGameTimer.Elapsed.TotalSeconds > START_GAME_TIME)
            {
                StartGame();
            }
            _camera.Update(gameTime);
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _camera.View);
            GraphicsHandler.DrawSprites(_spriteList, _spriteBatch);
            _spriteBatch.End();
        }

        public void OnChange(Object o)
        {
            _camera.ResetCamera();
            _camera.Zoom = START_ZOOM;
            _camera.Position = Utils.GetScreenCenter(_spriteBatch.GraphicsDevice);
            _camera.Jump2Target();

            _chosenBodyPart = ChooseHealthyBodyPart();
            _bodyPartPickTimer.Restart();
            _zoomTimer.Reset();
            _startGameTimer.Restart();
        }
    }
}
