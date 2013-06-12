using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects.Strategies;

namespace SommarFenomen
{
    abstract class ActiveGameObject : IGameObject
    {
        private static readonly float DEFAULT_MAX_SPEED = 250000;

        private Vector2 speed = Vector2.Zero;
        public Vector2 PreviousPosition { get; set; }
        private BoundingRect bounds;
        public double MaxSpeed { get; set; }
        public IStrategy Strategy { get; set; }

        public ActiveGameObject(IStrategy strategy, double maxSpeed)
        {
            Strategy = strategy;
            MaxSpeed = maxSpeed * maxSpeed;

            bounds.Min = Vector2.Zero;
            bounds.Max = Utils.AddToVector(Bounds.Min, 1);
        }

        public ActiveGameObject(IStrategy strategy)
        {
            Strategy = strategy;
            MaxSpeed = DEFAULT_MAX_SPEED;

            bounds.Min = Vector2.Zero;
            bounds.Max = Utils.AddToVector(Bounds.Min, 1);
        }

        public ActiveGameObject()
        {
            Strategy = new StationaryStrategy();
            MaxSpeed = DEFAULT_MAX_SPEED;

            bounds.Min = Vector2.Zero;
            bounds.Max = Utils.AddToVector(Bounds.Min, 1);
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
            get { return bounds.Min; }
            set { this.bounds.Move(value); }
        }

        public Vector2 getCenter()
        {
            return new Vector2(bounds.Min.X + bounds.Width / 2, Bounds.Min.Y + bounds.Height / 2);
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

        public abstract void Draw(SpriteBatch batch);

        public virtual void Update(GameTime gameTime)
        {
            AddAcceleration(Strategy.GetAcceleration(), gameTime);

            PreviousPosition = Position;
            Position += speed * (float)Utils.TicksToSeconds(gameTime.ElapsedGameTime.Ticks);
        }

        protected void setBoundsFromSprite(Sprite sprite)
        {
            bounds.Max.X = bounds.Left + sprite.ScaledSize.X;
            bounds.Max.Y = bounds.Top + sprite.ScaledSize.Y;
        }
    }
}
