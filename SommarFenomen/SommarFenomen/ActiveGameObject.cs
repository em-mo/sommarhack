using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SommarFenomen
{
    class ActiveGameObject : IGameObject
    {
        private Vector2 speed = Vector2.Zero;
        private Vector2 position;
        private BoundingRect bounds;
        private Sprite sprite;

        private IStrategy strategy;

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

        public virtual void draw(GraphicsHandler g)
        {
            g.DrawSprite(sprite);
        }
        public virtual void update(GameTime gameTime)
        {
            speed += strategy.getAcceleration() * gameTime.ElapsedGameTime.Seconds;
            position += speed * gameTime.ElapsedGameTime.Seconds;
        }
    }
}
