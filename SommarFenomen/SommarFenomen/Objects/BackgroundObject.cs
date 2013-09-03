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
        public BackgroundObject(Texture2D texture, Vector2 position, float rotation, float size, Strategy strategy)
        {
            _sprite = new Sprite(texture);
            Position = position;
            _sprite.Position = position;
            _sprite.Scale = new Vector2(size);
            _sprite.Color = ColorFromSize(size);
            _rotation = rotation;
            _strategy = strategy;
        }

        private Color ColorFromSize(float size)
        {
            int colorValue = (int)(255 * size);
            colorValue = (colorValue > 255) ? 255 : colorValue;
            return new Color(colorValue, colorValue, colorValue, colorValue);
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
