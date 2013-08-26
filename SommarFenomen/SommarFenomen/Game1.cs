using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using SommarFenomen.Util;

namespace SommarFenomen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public static ContentManager contentManager;

        private SpriteBatch _spriteBatch;
        private WindowHandler _windowHandler;
        private Thread _kinectThread;
        private KinectHandler _kinectHandler;

        private const float FPS = 60;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this); 
            contentManager = new ContentManager(Services);
            contentManager.RootDirectory = "Content";
            _windowHandler = new WindowHandler(this);

            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / FPS);

            graphics.PreferredBackBufferWidth = 1280;//GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width *3 / 4;
            graphics.PreferredBackBufferHeight = 720; // GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 3 / 4;

            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            ConvertUnits.SetDisplayUnitToSimUnitRatio(100f);
            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            PlayWindow.LoadContent();
            PlayWindow playWindow = new PlayWindow(_windowHandler);
            playWindow.Initialize();
            _windowHandler.ChangeWindow(playWindow);

            _kinectHandler = new KinectHandler(playWindow);
            _kinectThread = new Thread(() => _kinectHandler.run());
            _kinectThread.IsBackground = true;
            _kinectThread.Start();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (!checkExitKey(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One)))
            {
                _windowHandler.UpdateWindow(gameTime);
                base.Update(gameTime);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Color color = new Color();

            color.R = 120;
            color.G = 20;
            color.B = 20;

            GraphicsDevice.Clear(color);
            _windowHandler.DrawWindowGraphics(gameTime);
            base.Draw(gameTime);
        }

        bool checkExitKey(KeyboardState keyboardState, GamePadState gamePadState)
        {
            // Check to see whether ESC was pressed on the keyboard 
            // or BACK was pressed on the controller.
            if (keyboardState.IsKeyDown(Keys.Escape) ||
                gamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
                return true;
            }
            return false;
        }
    }
    


    static class Shared
    {
        public static readonly Random Random = new Random();
    }
}
