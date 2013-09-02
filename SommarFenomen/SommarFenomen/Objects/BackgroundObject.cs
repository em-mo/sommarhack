using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen.Objects
{
    class BackgroundObject : IGameObject
    {
        private Sprite _sprite;
        private float _rotation;
        private Strategy _strategy;
        public Vector2 Position { get; set; }
        public BackgroundObject(Texture2D texture, Vector2 position, float rotation, Strategy strategy)
        {
            _sprite = new Sprite(texture);
            Position = position;
            _sprite.Position = position;
            _rotation = rotation;
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Position += (float)gameTime.ElapsedGameTime.TotalSeconds * _strategy.GetAcceleration();
            _sprite.Position = Position;
            _sprite.Rotation += (float)gameTime.ElapsedGameTime.TotalSeconds * _rotation;
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {
            GraphicsHandler.DrawSprite(_sprite, batch);
        }
    }
}
