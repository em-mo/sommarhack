using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Kinect;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using SommarFenomen.Objects;
using SommarFenomen.Util;
using FarseerPhysics.Dynamics;
using FarseerPhysics.DebugViews;
using FarseerPhysics;
using FarseerPhysics.Factories;

namespace SommarFenomen
{
    class PlayWindow : Window
    {
        public World World { get; set; }
        private DebugViewXNA _debugView;
        private WindowHandler _windowHandler;

        private Sprite _background;
        private SpriteBatch _spriteBatch;
        private GraphicsDevice _graphicsDevice;
        private Camera2D _camera2D;
        private KeyboardInputHelper _inputHelper;

        private List<Sprite> _spriteList = new List<Sprite>();
        private List<Sprite> _backgroundSprites = new List<Sprite>();

        public List<GoodCell> GoodCellList { get; set; }

        private List<ActiveGameObject> _objectList = new List<ActiveGameObject>();

        Random rand = new Random();

        // Chooses algorithm for hand movement
        private bool _movementType = true;

        private PlayerCell _player;

        public PlayWindow(WindowHandler windowHandler)
        {
            this._windowHandler = windowHandler;
            GoodCellList = new List<GoodCell>();
        }

        public void Initialize()
        {
            _inputHelper = new KeyboardInputHelper();
            this._graphicsDevice = _windowHandler.Game.GraphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _camera2D = new Camera2D(_graphicsDevice);

            World = new World(Vector2.Zero);

            InitDebug();

            _background = new Sprite(Game1.contentManager.Load<Texture2D>(@"Images\Gradient"));
            _background.CenterOrigin(); 

            _backgroundSprites.Add(_background);

            _objectList = new List<ActiveGameObject>();

            _player = new PlayerCell(this, new Vector2(0));

            //Body body = BodyFactory.CreateRectangle(World, 2, 2, 2);
            //body.BodyType = BodyType.Static;

            GoodCell goodCell = new GoodCell(this, new Vector2(300, 100));
            GoodCellList.Add(goodCell);
            _objectList.Add(goodCell);
            _objectList.Add(new Virus(this, new Vector2(-200)));

            _camera2D.EnableTracking = true;
            _camera2D.TrackingBody = _player.Body;
        }

        static public void LoadContent()
        {
            GoodCell.LoadContent();
            Virus.LoadContent();
        }

        private void InitDebug()
        {
            _debugView = new DebugViewXNA(World);
            _debugView.RemoveFlags(DebugViewFlags.Shape);
            _debugView.RemoveFlags(DebugViewFlags.Joint);
            _debugView.DefaultShapeColor = Color.White;
            _debugView.SleepingShapeColor = Color.LightGray;
            _debugView.LoadContent(_graphicsDevice, Game1.contentManager);
        }
        
        public PlayerCell Player
        {
            get { return _player; }
        }

        public bool MovmentType
        {
            get { return _movementType; }
        }

        private void ToggleMovementType()
        {
            if (_movementType)
                _movementType = false;
            else
                _movementType = true;

            System.Console.WriteLine(_movementType);
        }

        private void KeyboardInput()
        {
            #region Key States

            if (_inputHelper.isKeyPressed(Keys.Space))
            {
                ToggleMovementType();
            }
            if (_inputHelper.isKeyPressed(Keys.F1))
            {
                _debugView.Flags = _debugView.Flags ^ DebugViewFlags.Shape;
            }
            if (_inputHelper.isKeyPressed(Keys.F2))
            {
                _debugView.Flags = _debugView.Flags ^ DebugViewFlags.DebugPanel;
                _debugView.Flags = _debugView.Flags ^ DebugViewFlags.PerformanceGraph;
            }
            if (_inputHelper.isKeyDown(Keys.Q))
                _camera2D.Zoom += 0.01f;
            if (_inputHelper.isKeyDown(Keys.W))
                _camera2D.Zoom -= 0.01f;
            if (_inputHelper.isKeyDown(Keys.R))
                _camera2D.Zoom = 1.0f;
            #endregion
        }

        public void Update(GameTime gameTime)
        {
            World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));            

            _inputHelper.update();

            _player.Update(gameTime);

            foreach (var gameObject in _objectList)
            {
                gameObject.Update(gameTime);
            }

            _camera2D.Update(gameTime);
            KeyboardInput();
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _camera2D.View);
            GraphicsHandler.DrawSprites(_backgroundSprites, _spriteBatch);
            GraphicsHandler.DrawSprites(_spriteList, _spriteBatch);
            _player.Draw(_spriteBatch);

            foreach (var gameObject in _objectList)
            {
                gameObject.Draw(_spriteBatch);
            }

            _spriteBatch.End();
            
            Matrix projection = _camera2D.SimProjection;
            Matrix view = _camera2D.SimView;
            _debugView.RenderDebugData(ref projection, ref view);
        }
    }
}
