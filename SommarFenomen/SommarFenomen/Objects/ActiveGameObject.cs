using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects.Strategies;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using SommarFenomen.Util;

namespace SommarFenomen.Objects
{
    abstract class ActiveGameObject : IGameObject
    {
        private static readonly float DEFAULT_MAX_SPEED = 250000;

        public PlayWindow PlayWindow;

        public Vector2 Position { get; set; }
        public double MaxSpeed { get; set; }
        public Strategy Strategy { get; set; }
        public Sprite Sprite { get; set; }

        public Body Body { get; set; }
        public Fixture Fixture { get; set; }

        public ActiveGameObject(PlayWindow playWindow, Strategy strategy, double maxSpeed)
        {
            PlayWindow = playWindow;
            Strategy = strategy;
            MaxSpeed = maxSpeed * maxSpeed;
        }

        public ActiveGameObject(PlayWindow playWindow, Strategy strategy)
        {
            PlayWindow = playWindow;
            Strategy = strategy;
            MaxSpeed = DEFAULT_MAX_SPEED;
        }

        public ActiveGameObject(PlayWindow playWindow)
        {
            PlayWindow = playWindow;
            Strategy = new StationaryStrategy();
            MaxSpeed = DEFAULT_MAX_SPEED;
        }

        public Vector2 Speed
        {
            get { return Body.LinearVelocity; }
            set { Body.LinearVelocity = value; } 
        }

        protected void UpdateSpriteFromBody(Sprite sprite)
        {
            sprite.Position = ConvertUnits.ToDisplayUnits(Body.Position);
            Sprite.Rotation = Body.Rotation;
        }
        
        protected void UpdateSpriteFromBody()
        {
            Sprite.Position = ConvertUnits.ToDisplayUnits(Body.Position);
            Sprite.Rotation = Body.Rotation;
        }

        public virtual void Draw(SpriteBatch batch)
        {
            UpdateSpriteFromBody();
            GraphicsHandler.DrawSprite(Sprite, batch);
        }

        public virtual void Update(GameTime gameTime)
        {
            Body.ApplyForce(Strategy.GetAcceleration());
            Position = ConvertUnits.ToDisplayUnits(Body.Position);

        }

        public abstract void CreateBody();
        public abstract bool ObjectCollision(Fixture f1, Fixture f2, Contact contact);

    }
}
