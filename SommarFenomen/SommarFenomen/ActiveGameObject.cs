using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen
{
    class ActiveGameObject : IGameObject
    {
        private Vector2 speed = Vector2.Zero;
        private Vector2 position;
        private Vector2 previousPosition;
        private BoundingRect bounds;
        public double MaxSpeed { get; set; }
        public IStrategy Strategy { get; set; }

        public ActiveGameObject(IStrategy strategy, double maxSpeed)
        {
            Strategy = strategy;
            MaxSpeed = maxSpeed * maxSpeed;
        }

        public Vector2 Speed
        {
            get { return speed; }
            set { this.speed = value; } 
        }

        public BoundingRect Bounds
        {
            get { return bounds; }
            set { this.bounds = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { this.position = value; }
        }

        public void AddAcceleration(Vector2 acceleration, GameTime gameTime)
        {
            speed += acceleration * (float)Utils.TicksToSeconds(gameTime.ElapsedGameTime.Ticks);

            double absoluteSpeed = (Math.Pow(speed.X, 2) + Math.Pow(speed.Y, 2));
            if (absoluteSpeed > MaxSpeed)
            {
                float overRatio = (float)Math.Sqrt(MaxSpeed / absoluteSpeed);
                speed *= overRatio;
            }
        }

        public virtual void Draw(SpriteBatch batch)
        {
        }

        public virtual void Update(GameTime gameTime)
        {
            AddAcceleration(Strategy.GetAcceleration(), gameTime);

            previousPosition = position;
            position += speed * (float)Utils.TicksToSeconds(gameTime.ElapsedGameTime.Ticks);
        }
    }
}
