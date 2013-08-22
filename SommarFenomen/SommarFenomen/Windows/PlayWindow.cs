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
using FarseerPhysics.Common;
using SommarFenomen.LevelHandling;
using SommarFenomen.Windows;

namespace SommarFenomen
{
    class PlayWindow : Window
    {
        public World World { get; set; }
        private DebugViewXNA _debugView;
        private WindowHandler _windowHandler;

        private Background _background2;
        private Sprite _background;
        private SpriteBatch _spriteBatch;
        public GraphicsDevice GraphicsDevice { get; set; }
        public Camera2D Camera2D;
        private KeyboardInputHelper _inputHelper;

        private List<Sprite> _spriteList = new List<Sprite>();
        private List<Sprite> _backgroundSprites = new List<Sprite>();

        private List<ActiveGameObject> _removeList = new List<ActiveGameObject>();
        private List<ActiveGameObject> _addList = new List<ActiveGameObject>();

        public List<GoodCell> GoodCellList { get; set; }
        private List<ActiveGameObject> _objectList = new List<ActiveGameObject>();

        private List<Wall> _wallList = new List<Wall>();

        private Level _level;
        private LevelParser _levelParser;

        Random rand = new Random();

        Stopwatch mouseWatch = new Stopwatch();

        // Chooses algorithm for hand movement
        private bool _movementType = true;

        private PlayerCell _player;

        public PlayWindow(WindowHandler windowHandler)
        {
            this._windowHandler = windowHandler;
            GoodCellList = new List<GoodCell>();
            _levelParser = new LevelParser(this);

            mouseWatch.Start();
        }

        public void Initialize()
        {
            _inputHelper = new KeyboardInputHelper();
            this.GraphicsDevice = _windowHandler.Game.GraphicsDevice;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Camera2D = new Camera2D(GraphicsDevice);
            World = new World(Vector2.Zero);

            InitDebug();

            _background = new Sprite(Game1.contentManager.Load<Texture2D>(@"Images\Gradient"));
            _background.CenterOrigin();
            _backgroundSprites.Add(_background);

            _background2 = new Background(@"Images\Gradient", this);

            //TestInit();
            LoadLevel(@"levels\smalltest");

            Camera2D.EnableTracking = true;
            Camera2D.TrackingBody = _player.Body;
        }

        private void TestInit()
        {
            World = new World(Vector2.Zero);

            _objectList = new List<ActiveGameObject>();

            _player = new PlayerCell(this, new Vector2(0));

            GoodCell goodCell = new GoodCell(this, new Vector2(300, 100));
            GoodCellList.Add(goodCell);
            _objectList.Add(goodCell);
            _objectList.Add(new Virus(this, new Vector2(200, -200)));


            //Vertices vertices = new Vertices();
            //vertices.Add(new Vector2(-300, -200));
            //vertices.Add(new Vector2(-200, -150));
            //vertices.Add(new Vector2(-100, -200));
            //vertices.Add(new Vector2(-160, -300));
            //vertices.Add(new Vector2(-240, -300));

            //_wallList.Add(new Wall(vertices, Wall.WallType.Outer, this));

            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-200, 200));
            vertices.Add(new Vector2(-150, 225));
            vertices.Add(new Vector2(-100, 200));
            vertices.Add(new Vector2(-125, 250));
            vertices.Add(new Vector2(-100, 300));
            vertices.Add(new Vector2(-150, 275));
            vertices.Add(new Vector2(-200, 300));
            vertices.Add(new Vector2(-175, 250));

            _wallList.Add(new Wall(vertices, Wall.WallType.Outer, this));
        }

        private void LoadLevel(String file)
        {
            World.Clear();

            _level = _levelParser.Parse(Game1.contentManager.Load<Texture2D>(file));
            _wallList = _level.GetWalls();
            _objectList = _level.GetFriendlies();

            foreach (var item in _objectList)
            {
                GoodCellList.Add((GoodCell)item);
            }
            _objectList.AddRange(_level.GetEnemies());

            _player = _level.Player;
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
            _debugView.LoadContent(GraphicsDevice, Game1.contentManager);
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

        public void RegisterVirus(Virus virus)
        {
            _addList.Add(virus);
        }

        public void RemoveVirus(Virus virus)
        {
            _removeList.Add(virus);
            virus.UpForRemoval = true;
        }

        public void RegisterGoodCell(GoodCell goodCell)
        {
            _addList.Add(goodCell);
        }

        public void RemoveGoodCell(GoodCell goodCell)
        {
            GoodCellList.Remove(goodCell);
            _removeList.Add(goodCell);
            goodCell.UpForRemoval = true;
        }

        private void RegisterGameObjects()
        {
            foreach (var item in _addList)
            {
                if (item is Virus)
                {
                    _objectList.Add(item);
                }
                else if (item is GoodCell)
                {
                    GoodCellList.Add((GoodCell)item);
                    _objectList.Add(item);
                }
            }
            _addList.Clear();
        }

        private void DestroyOldGameObjects()
        {
            foreach (var item in _removeList)
            {
                _objectList.Remove(item);
                World.RemoveBody(item.Body);
            }
            _removeList.Clear();
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
            if (_inputHelper.isKeyPressed(Keys.F3))
            {
                _debugView.Flags = _debugView.Flags ^ DebugViewFlags.Joint;
            }
            if (_inputHelper.isKeyDown(Keys.Q))
                Camera2D.Zoom += 0.01f;
            if (_inputHelper.isKeyDown(Keys.W))
                Camera2D.Zoom -= 0.01f;
            if (_inputHelper.isKeyDown(Keys.R))
                Camera2D.Zoom = 1.0f;
            #endregion
        }

        public void Update(GameTime gameTime)
        {
            World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));            

            _inputHelper.update();

            _player.Update(gameTime);

            foreach (var gameObject in _objectList)
            {
                if(gameObject.UpForRemoval == false)
                    gameObject.Update(gameTime);
            }

            Camera2D.Update(gameTime);
            KeyboardInput();
            DestroyOldGameObjects();
            RegisterGameObjects();

            if (mouseWatch.ElapsedMilliseconds > 1000)
            {
                mouseWatch.Restart();
                var mouseState = Mouse.GetState();

                //Vector2 mousePos = ConvertUnits.ToDisplayUnits(Camera2D.ConvertScreenToWorld(new Vector2(mouseState.X, mouseState.Y)));
                Vector2 mousePos = Camera2D.ConvertScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                
                //Console.WriteLine("MousePos: " + mousePos);
            }
        }

        public void Draw(GameTime gameTime)
        {
            _background2.Draw(_spriteBatch);
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Camera2D.View);
            //GraphicsHandler.DrawSprites(_backgroundSprites, _spriteBatch);
            GraphicsHandler.DrawSprites(_spriteList, _spriteBatch);
            _player.Draw(_spriteBatch);

            foreach (var gameObject in _objectList)
            {
                gameObject.Draw(_spriteBatch);
            }

            _spriteBatch.End();

            foreach (var item in _wallList)
            {
                item.Draw(_spriteBatch);
            }
            
            Matrix projection = Camera2D.SimProjection;
            Matrix view = Camera2D.SimView;
            _debugView.RenderDebugData(ref projection, ref view);
        }
    }
}
