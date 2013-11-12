using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using SommarFenomen.Util;
using SommarFenomen.Stats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SommarFenomen.Windows
{
    class ScoreWindow : Window
    {
        private KinectHandler _kinectHandler;
        private WindowHandler _windowHandler;
        private KeyboardInputHelper _inputHelper;
        private SpriteBatch _spriteBatch;
        private SpriteFont _titleFont;
        private SpriteFont _entryFont;
        private Sprite _background;
        private string _titleString;
        private Vector2 _titlePosition;
        private Vector2 _entryPositions;
        private Highscore _highScore;

        private Stopwatch _skipTimer = new Stopwatch();
        private static readonly double SKIP_TIME = 10.0;

        public ScoreWindow(WindowHandler windowHandler)
        {
            _inputHelper = new KeyboardInputHelper();
            _windowHandler = windowHandler;
            _spriteBatch = new SpriteBatch(_windowHandler.Game.GraphicsDevice);
            _titleFont = Game1.contentManager.Load<SpriteFont>(@"ScoreHeading");
            _entryFont = Game1.contentManager.Load<SpriteFont>(@"ScoreEntries");
            _titleString = "Snabbaste tiderna";
            _background = new Sprite(Game1.contentManager.Load<Texture2D>(@"images\highscorebackground"));
            Vector2 scale;
            scale.X = _windowHandler.Game.GraphicsDevice.Viewport.Width / _background.OriginalSize.X;
            scale.Y = _windowHandler.Game.GraphicsDevice.Viewport.Height / _background.OriginalSize.Y;
            _background.Scale = scale;

            Vector2 stringDimensions = _titleFont.MeasureString(_titleString);
            _titlePosition = new Vector2((_windowHandler.Game.GraphicsDevice.Viewport.Width - stringDimensions.X) / 2,
                                _windowHandler.Game.GraphicsDevice.Viewport.Height / 7);

            _entryPositions = new Vector2(_windowHandler.Game.GraphicsDevice.Viewport.Width / 4,
                                        (_windowHandler.Game.GraphicsDevice.Viewport.Height * 2) / 7);
        }

        public void Initialize()
        {
        }

        private object _nextWindowObject;
        public void OnChange(object o)
        {
            _nextWindowObject = o;
            _highScore = Shared.StatsHandler.GetHighscores();
            _skipTimer.Restart();

            _scoreStrings.Clear();
            _nameStrings.Clear();

            foreach (var score in _highScore._scores)
            {
                _nameStrings.Add(score.Name);
                _scoreStrings.Add((score.TimeTicks / TimeSpan.TicksPerSecond).ToString());
            }
        }

        public void HandleInput()
        {
            Keys[] pressedKeys = _inputHelper.GetKeyDowns();

            if (_inputHelper.isKeyDown(Keys.Enter))
            {
                Finish();
                return;
            }

            if (pressedKeys.Length > 0)
            {
                _skipTimer.Restart();

                Score score = _highScore.GetLatestScore();
                StringBuilder name = new StringBuilder(score.Name);

                foreach (var key in pressedKeys)
                {
                    Console.WriteLine(key.ToString());
                    if (key == Keys.Back)
                    {
                        if (name.Length > 0)
                            name.Remove(name.Length - 1, 1);
                    }
                    else if (name.Length < 10)
                    {
                        if (key == Keys.Space)
                            name.Append(' ');
                        else if (key == Keys.OemCloseBrackets)
                            name.Append('Å');
                        else if (key == Keys.OemQuotes)
                            name.Append('Ä');
                        else if (key == Keys.OemTilde)
                            name.Append('Ö');
                        else
                        {
                            string keyString = key.ToString();
                            if (keyString.Length == 1)
                            {
                                name.Append(keyString[0]);
                            }
                        }
                    }
                }
                score.Name = name.ToString();
                _nameStrings[_highScore.GetLatestScoreIndex()] = score.Name;
            }
        }

        private void Finish()
        {
            Shared.StatsHandler.Save();
            _windowHandler.ChangeWindow(_windowHandler.WaitingWindow, _nextWindowObject);
        }

        List<string> _scoreStrings = new List<string>();
        List<string> _nameStrings = new List<string>();

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _inputHelper.update();

            if (_skipTimer.Elapsed.TotalSeconds > SKIP_TIME)
            {
                Finish();
            }

            HandleInput();
        }

        Color textColor = new Color(80, 80, 80);
        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _spriteBatch.GraphicsDevice.Clear(Color.RosyBrown);
            _spriteBatch.Begin();

            GraphicsHandler.DrawSprite(_background, _spriteBatch);

            _spriteBatch.DrawString(_titleFont, _titleString, _titlePosition, Color.Black);

            Vector2 currentPosition = _entryPositions;
            Vector2 offsetX = new Vector2(_entryPositions.X * 1.5f, 0);
            Vector2 offsetY = Vector2.Zero;
            offsetY.Y = _entryFont.MeasureString("Test").Y * 0.9f;

            for (int i = 0; i < _nameStrings.Count; i++)
            {
                Color color = textColor;
                if (_highScore.GetLatestScoreIndex() == i)
                    color = Color.Gold;

                _spriteBatch.DrawString(_entryFont, _nameStrings[i], currentPosition, color);
                
                _spriteBatch.DrawString(_entryFont, _scoreStrings[i], currentPosition + offsetX, color);

                currentPosition += offsetY;
            }

            _spriteBatch.End();
        }
    }
}
