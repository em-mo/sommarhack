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
        private Dictionary<BodyPartType, Vector2> BodyPartPositions = new Dictionary<BodyPartType, Vector2>();
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

            Vector2 imageScaledSize = _background.ScaledSize / 2;

            HEART_POSITION = Utils.GetScreenCenter(graphicsDevice);
            KNEE_POSITION = Utils.GetScreenCenter(graphicsDevice);
            NECK_POSITION = Utils.GetScreenCenter(graphicsDevice);
            BRAIN_POSITION = Utils.GetScreenCenter(graphicsDevice);

            HEART_POSITION.X += imageScaledSize.X * -0.005f;
            HEART_POSITION.Y += imageScaledSize.Y * -0.41f;

            KNEE_POSITION.X += imageScaledSize.X * 0.06f;
            KNEE_POSITION.Y += imageScaledSize.Y * 0.43f;

            NECK_POSITION.X += imageScaledSize.X * -0.01f;
            NECK_POSITION.Y += imageScaledSize.Y * -0.65f;

            BRAIN_POSITION.X += imageScaledSize.X * -0.01f;
            BRAIN_POSITION.Y += imageScaledSize.Y * -0.85f;

            BodyPartPositions[BodyPartType.BRAIN] = BRAIN_POSITION;
            BodyPartPositions[BodyPartType.HEART] = HEART_POSITION;
            BodyPartPositions[BodyPartType.NECK] = NECK_POSITION;
            BodyPartPositions[BodyPartType.KNEE] = KNEE_POSITION;
            
        }

        private static Vector2 HEART_POSITION;
        private static Vector2 BRAIN_POSITION;
        private static Vector2 NECK_POSITION;
        private static Vector2 KNEE_POSITION;


        public void Initialize()
        {
            //LevelBodyPart part = new LevelBodyPart();
            //part.Position = BodyPartType.BRAIN;
            //part.LevelFiles.Add(@"levels\test");
            //_bodyParts.Add(part);
            _bodyParts = LevelBodyPart.LoadAllParts();
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
                _dot.Position = BodyPartPositions[_chosenBodyPart.Position];
                _spriteList.Add(_dot);
            }
            else if (_zoomTimer.Elapsed.TotalSeconds > ZOOM_TIME)
            {
                _camera.Position = BodyPartPositions[_chosenBodyPart.Position];
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
            Color color = new Color(247, 118, 97);
            _spriteBatch.GraphicsDevice.Clear(color);
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
