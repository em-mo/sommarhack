using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen
{
    class DeathFactory
    {
        private Sprite FactorySprite = new Sprite();
        private static Texture2D FactoryTexture;

        private double nextCloudTime;
        private double cloudTimer;

        private const double CLOUD_SPAWN_AVERAGE = 5000;
        private const double CLOUD_SPAWN_DEVIATION = 1000;

        public Vector2 Position
        {
            get { return FactorySprite.Position; }
            set { FactorySprite.Position = value; }
        }

        // Reference for cloud spawning
        private GameWindow game;

        /// <summary>
        /// Factory that spews out poison!
        /// </summary>
        /// <param name="position"></param>
        /// <param name="game"></param>
        public DeathFactory(GameWindow game)
        {
            FactorySprite.Initialize();
            FactorySprite.Texture = FactoryTexture;
            FactorySprite.Scale = new Vector2(0.5f, 0.5f);

            this.game = game;

            nextCloudTime = GetNextCloudTime();
        }

        public static void LoadContent()
        {
            FactoryTexture = Game1.contentManager.Load<Texture2D>(@"Images/Factory_1");
        }

        /// <summary>
        /// Gets a random number between average +- deviation
        /// </summary>
        /// <returns></returns>
        private double GetNextCloudTime()
        {
            return CLOUD_SPAWN_AVERAGE + (Shared.Random.NextDouble() - Shared.Random.NextDouble()) * CLOUD_SPAWN_DEVIATION;
        }

        public Vector2 GetSize()
        {
            return FactorySprite.Size;
        }

        public void Update(GameTime gameTime)
        {
            cloudTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (cloudTimer > nextCloudTime)
            {
                cloudTimer -= nextCloudTime;
                nextCloudTime = GetNextCloudTime();

                Vector2 poisonPosition = FactorySprite.Position;
                poisonPosition.Y -= FactoryTexture.Height / 4;

                game.AddPoisonCloud(poisonPosition);
            }
        }

        public void Draw(GraphicsHandler g)
        {
            g.DrawSprite(FactorySprite);
        }
    }
}
