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

namespace SommarFenomen
{
    class GameWindow : Window
    {
        Sprite background;
        GraphicsHandler graphicsHandler;
        KeyboardState oldState;

        List<Sprite> raindropsList = new List<Sprite>();
        List<Plant> plantList = new List<Plant>();
        List<Sprite> spriteList = new List<Sprite>();
        List<Sprite> backgroundSprites = new List<Sprite>();
        List<PoisonCloud> poisonCloudList = new List<PoisonCloud>();
        List<DeathFactory> deathFactoryList = new List<DeathFactory>();

        SoundEffect notCarrie;
        SoundEffectInstance notCarrieInstance;

        // Raindrops
        private int dropDelay = 300;
        private const float DROP_SPEED = 200;

        // Speed for first algorithm
        private const int MOVE_SPEED = 135;
        // Speed for second algorithm
        private const int ALTERNATE_MOVE_SPEED = 40;
        private float PLANT_RESET_TIME = 2000;

        private Stopwatch plantResetCounter = new Stopwatch();

        private Stopwatch rainDropsTimer = new Stopwatch();
        Random rand = new Random();

        public readonly object dropLock = new object();

        // Chooses algorithm for hand movement
        private bool movementType = true;

        private Player player;

        public Player Player
        {
            get { return player; }
        }

        public bool MovmentType
        {
            get { return movementType; }
        }

        private void ToggleMovementType()
        {
            if (movementType)
                movementType = false;
            else
                movementType = true;

            System.Console.WriteLine(movementType);
        }

        public void Initialize(SpriteBatch batch)
        {
            rainDropsTimer.Start();

            Plant plant = new Plant();
            plant.Position = new Vector2(Game1.graphics.PreferredBackBufferWidth / 8, Game1.graphics.PreferredBackBufferHeight - plant.GetSize().Y);
            plantList.Add(plant);

            Plant plant2 = new Plant();
            plant2.Position = new Vector2(Game1.graphics.PreferredBackBufferWidth * 6    / 8, Game1.graphics.PreferredBackBufferHeight - plant2.GetSize().Y);
            plantList.Add(plant2);

            
            DeathFactory factory = new DeathFactory(this);
            factory.Position = new Vector2(Game1.graphics.PreferredBackBufferWidth / 2, Game1.graphics.PreferredBackBufferHeight - factory.GetSize().Y);
            deathFactoryList.Add(factory);

            graphicsHandler = new GraphicsHandler();
            graphicsHandler.Initialize(batch);
            oldState = new KeyboardState();

            background = new Sprite();
            background.Initialize();
            background.Texture = Game1.contentManager.Load<Texture2D>(@"Images\Gradient");
            background.Size = new Vector2(Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
            background.Layer = 0;

            player = new Player(background.Size);

            backgroundSprites.Add(background);

            notCarrie = Game1.contentManager.Load<SoundEffect>(@"Sounds/carrie2");
            notCarrieInstance = notCarrie.CreateInstance();
        }

        public void StartNotCarrie()
        {
            if (notCarrieInstance.State == SoundState.Stopped)
            {
                notCarrieInstance.Volume = 0.75f;
                notCarrieInstance.IsLooped = true;
                notCarrieInstance.Play();
            }
            else if (notCarrieInstance.State == SoundState.Paused)
                notCarrieInstance.Resume();
        }

        public void StopNotCarrie()
        {
            if (notCarrieInstance.State == SoundState.Playing)
                notCarrieInstance.Pause();
        }

        private bool[] keysDown = new bool[5];

        public void Update(GameTime gameTime)
        {
            player.Update(gameTime);
            UpdateFallingRaindrops(gameTime);
            UpdateFactories(gameTime);
            UpdatePoisonClouds(gameTime);
            CheckForResetFlowers();
            CheckForCloudCollision();

            #region Key States
            KeyboardState newState = Keyboard.GetState();
            
            //KEY DOWN
            if (newState.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down))
            {
                SwipeDown(Arm.Left);
            }

            //KEY UP
            if (newState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up))
            {
                SwipeUp(Arm.Right);
            }

            //KEY RIGHT
            if (newState.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Right))
            {
                SwipeRight(Arm.Right);
            }

            //KEY LEFT
            if (newState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Left))
            {
                SwipeLeft(Arm.Left);
            }

            //KEY SPACE
            if (newState.IsKeyDown(Keys.Space) && !oldState.IsKeyDown(Keys.Space))
            {
                ToggleMovementType();
            }

            oldState = newState;
            #endregion
        }

        /// <summary>
        /// Checks for collisions between player and poison clouds
        /// </summary>
        private void CheckForCloudCollision()
        {
            BoundingRect playerRect = player.GetBounds();
        }

        /// <summary>
        /// Resets clouds if all plants are fully grown
        /// </summary>
        private void CheckForResetFlowers()
        {
            bool reset = true;
            foreach (Plant plant in plantList)
            {
                if (plant.GetGrowthStage() != 2)
                {
                    reset = false;
                    break;
                }
            }

            if (reset)
            {
                plantResetCounter.Start();
            }

            if (plantResetCounter.ElapsedMilliseconds > PLANT_RESET_TIME)
            {
                foreach (Plant plant in plantList)
                {
                    plant.Reset();
                }
                plantResetCounter.Stop();
                plantResetCounter.Reset();
            }
        }

        public void AddPoisonCloud(Vector2 position)
        {
            poisonCloudList.Add(new PoisonCloud(position));
        }

        /// <summary>
        /// creates a new raindrop under the cloud after every interval (defined by dropDelay)
        /// </summary>
        public void releaseRainDrops()
        {
            if (rainDropsTimer.ElapsedMilliseconds > dropDelay)
            {
                Sprite drop = new Sprite();
                drop.Initialize();
                drop.Texture = Game1.contentManager.Load<Texture2D>(@"Images\Drop");
                float xValue = rand.Next((int)(player.Position.X +  0.2*player.GetSize().X) , (int)(player.Position.X + (player.GetSize().X) * 0.8));
                float yValue = player.Position.Y + player.GetSize().Y;
                drop.Position = new Vector2(xValue, yValue);

                lock(dropLock)
                    raindropsList.Add(drop);

                rainDropsTimer.Restart();
            }
        }

        public void UpdateFallingRaindrops(GameTime gameTime)
        {
            for (int i = raindropsList.Count - 1; i >= 0; i--)
            {
                lock (dropLock)
                {
                    Sprite drop = raindropsList.ElementAt(i);

                    drop.Position += new Vector2(0, DROP_SPEED * gameTime.ElapsedGameTime.Milliseconds / 1000);

                    foreach (Plant plant in plantList)
                    {
                        if (plant.CheckCollisionWithRaindrops(drop))
                            raindropsList.RemoveAt(i);
                    }

                    if (drop.Position.Y + drop.Size.Y >= background.Size.Y)
                            raindropsList.RemoveAt(i);
                }
            }
        }

        private void UpdateFactories(GameTime gameTime)
        {
            foreach (DeathFactory factory in deathFactoryList)
            {
                factory.Update(gameTime);
            }
        }

        private void UpdatePoisonClouds(GameTime gameTime)
        {
            PoisonCloud cloud;
            for (int i = poisonCloudList.Count - 1; i >= 0; i--)
            {
                cloud = poisonCloudList.ElementAt(i);
                cloud.Update(gameTime);

                if (cloud.OutOfBounds())
                    poisonCloudList.RemoveAt(i);
            }
        }

        public void SwipeUp(Arm arm)
        {
            lock (player.locker)
            {
                player.AddWindPuff((float)Math.PI / 2, arm);
                player.Speed += new Vector2(0, MOVE_SPEED);
            }
        }
        public void SwipeDown(Arm arm)
        {
            lock (player.locker)
            {
                player.AddWindPuff((float)-Math.PI / 2, arm);
                player.Speed += new Vector2(0, -MOVE_SPEED);
            }
        }
        public void SwipeLeft(Arm arm)
        {
            lock (player.locker)
            {
                player.AddWindPuff(0, arm);
                player.Speed += new Vector2(MOVE_SPEED, 0);
            }
        }
        public void SwipeRight(Arm arm)
        {
            lock (player.locker)
            {
                player.AddWindPuff((float)Math.PI, arm);
                player.Speed += new Vector2(-MOVE_SPEED, 0);
            }
        }

        /// <summary>
        /// For the movement type were many small pushes occur
        /// </summary>
        /// <param name="arm"></param>
        /// <param name="direction"></param>
        public void AlternativeSwipe(Arm arm, Direction direction)
        {
            if (direction == Direction.Up)
            {
                lock (player.locker)
                {
                    player.AddWindPuff((float)Math.PI / 2, arm);
                    player.Speed += new Vector2(0, ALTERNATE_MOVE_SPEED);
                }
            }
            else if (direction == Direction.Down)
            {
                lock (player.locker)
                {
                    player.AddWindPuff((float)-Math.PI / 2, arm);
                    player.Speed += new Vector2(0, -ALTERNATE_MOVE_SPEED);
                }
            }
            else if (direction == Direction.Left)
            {
                lock (player.locker)
                {
                    player.AddWindPuff(0, arm);
                    player.Speed += new Vector2(ALTERNATE_MOVE_SPEED, 0);
                }
            }
            else
            {
                lock (player.locker)
                {
                    player.AddWindPuff((float)Math.PI, arm);
                    player.Speed += new Vector2(-ALTERNATE_MOVE_SPEED, 0);
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            graphicsHandler.DrawSprites(backgroundSprites);
            foreach (Plant plant in plantList)
            {
                plant.Draw(graphicsHandler);
            }
            
            graphicsHandler.DrawSprites(raindropsList);
            graphicsHandler.DrawSprites(spriteList);
            player.Draw(graphicsHandler);

            foreach (PoisonCloud cloud in poisonCloudList)
            {
                cloud.Draw(graphicsHandler);
            }

            foreach (DeathFactory factory in deathFactoryList)
            {
                factory.Draw(graphicsHandler);
            }
        }
    }
}
