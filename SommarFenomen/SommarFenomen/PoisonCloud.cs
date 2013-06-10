using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen
{
    /// <summary>
    /// Cloud created by deathFactories that poisons the player
    /// </summary>
    class PoisonCloud
    {
        private Sprite poisonCloudSprite = new Sprite();
        private static Texture2D poisonCloudTexture;
        private Vector2 speed;
        private const int RANDOM_SPEED = 300;
        private const float SPEED_Y = 80;

        public Sprite GetSprite()
        {
            return poisonCloudSprite;
        }

        public PoisonCloud(Vector2 position)
        {
            poisonCloudSprite.Initialize();
            poisonCloudSprite.Texture = poisonCloudTexture;
            poisonCloudSprite.Position = position;
            poisonCloudSprite.Scale = new Vector2(0.25f, 0.25f);

            speed.Y = SPEED_Y;
            speed.X = Shared.Random.Next(-RANDOM_SPEED / 5, RANDOM_SPEED / 5);
        }

        public static void LoadContent()
        {
            poisonCloudTexture = Game1.contentManager.Load<Texture2D>(@"Images/dark-cloud");
        }

        private void RandomWalk(GameTime gameTime)
        {
            speed.X += (float)Shared.Random.Next(-RANDOM_SPEED, RANDOM_SPEED) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// True if out of bounds
        /// </summary>
        /// <returns></returns>
        public bool OutOfBounds()
        {
            return poisonCloudSprite.Position.Y < -poisonCloudSprite.Size.Y;
        }

        public void Update(GameTime gameTime)
        {
            RandomWalk(gameTime);

            poisonCloudSprite.Position -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(GraphicsHandler g)
        {
            g.DrawSprite(poisonCloudSprite);
        }


    }
}
