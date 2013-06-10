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

namespace SommarFenomen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public static ContentManager contentManager;

        SpriteBatch spriteBatch;
        WindowHandler windowHandler;
        Thread kinectThread;
        KinectHandler kinectHandler;

        private const float FPS = 200;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this); 
            contentManager = new ContentManager(Services);
            contentManager.RootDirectory = "Content";
            windowHandler = new WindowHandler();

            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / FPS);

            graphics.PreferredBackBufferWidth = 1280;//GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width *3 / 4;
            graphics.PreferredBackBufferHeight = 720; // GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 3 / 4;
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
            Plant.LoadContent();
            DeathFactory.LoadContent();

            PoisonCloud.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            GameWindow gameWindow = new GameWindow();
            gameWindow.Initialize(spriteBatch);
            windowHandler.ChangeWindow(gameWindow);

            kinectHandler = new KinectHandler(gameWindow);
            kinectThread = new Thread(() => kinectHandler.run());
            kinectThread.IsBackground = true;
            kinectThread.Start();
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            // TODO: Add your update logic here
            windowHandler.UpdateWindow(gameTime);
            base.Update(gameTime);
        }

        private Stopwatch stopWatch = new Stopwatch();
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.CornflowerBlue);
            windowHandler.DrawWindowGraphics(gameTime);
            base.Draw(gameTime);
            spriteBatch.End();
        }
    }

    static class Shared
    {
        public static readonly Random Random = new Random();
    }
}
