using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SommarFenomen.Objects;
using Microsoft.Xna.Framework;
using SommarFenomen.Util;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects.Strategies;

namespace SommarFenomen.Windows.WindowUtils
{
    class BackgroundObjectsHandler
    {
        private static readonly float OUTSIDE_DISTANCE = 300;
        private LinkedList<BackgroundObject> _objectsList = new LinkedList<BackgroundObject>();
        private Camera2D _camera;
        private int _halfHeight, _halfWidth;

        public BackgroundObjectsHandler(Camera2D camera)
        {
            _camera = camera;
            _halfWidth = Game1.graphics.GraphicsDevice.Viewport.Width / 2;
            _halfHeight = Game1.graphics.GraphicsDevice.Viewport.Height / 2;
        }

        private double spawnTimer;
        private double nextSpawn = SPAWN_TIME;
        private static readonly double SPAWN_TIME = 0.7;
        private static readonly double DEVIATION = 0.2;
        private void HandleBackgroundSpawning(GameTime gameTime)
        {
            spawnTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (spawnTimer > nextSpawn)
            {
                SpawnObject();
                spawnTimer = 0;
                nextSpawn = SPAWN_TIME + Shared.Random.NextDouble() * DEVIATION - DEVIATION / 2;
            }
        }

        private void SpawnObject()
        {
            Vector2 position = Vector2.Zero;
            Vector2 cameraPosition = _camera.Position;
            int side = Shared.Random.Next(4);
            int halfHeight = (int)(_halfHeight / _camera.Zoom);
            int halfWidth = (int)(_halfWidth / _camera.Zoom);
            switch (side)
            {
                case 0:
                    position.X = -halfWidth - OUTSIDE_DISTANCE / 2;
                    position.Y = Shared.Random.Next(halfHeight * 2) - halfHeight;
                    break;
                case 1:
                    position.X = halfWidth + OUTSIDE_DISTANCE / 2;
                    position.Y = Shared.Random.Next(halfHeight * 2) - halfHeight;

                    break;
                case 2:
                    position.X = Shared.Random.Next(halfWidth * 2) - halfWidth;
                    position.Y = -halfHeight - OUTSIDE_DISTANCE / 2;

                    break;
                case 3:
                    position.X = Shared.Random.Next(halfWidth * 2) - halfWidth;
                    position.Y = halfHeight + OUTSIDE_DISTANCE / 2;

                    break;
                default:
                    break;
            }
            position += _camera.Position;

            Texture2D texture = GoodCell._happyTextureCombinations[Shared.Random.Next(GoodCell._happyTextureCombinations.Count)];
            float rotation = (float)Shared.Random.NextDouble() * 0.6f;
            float size = (float)Shared.Random.NextDouble() * 0.1f;

            Vector2 velocity = cameraPosition - position;
            velocity.Normalize();
            velocity *= (float)(Shared.Random.NextDouble() * 50 + 25);

            _objectsList.AddLast(new BackgroundObject(texture, position, rotation, size, new StraightStrategy(velocity)));
        }

        private bool Outside(BackgroundObject o)
        {
            Vector2 offset = o.Position - _camera.Position;
            offset.X = Math.Abs(offset.X);
            offset.Y = Math.Abs(offset.Y);
            offset *= _camera.Zoom;
            if (offset.X > _halfWidth + OUTSIDE_DISTANCE || offset.Y > _halfHeight + OUTSIDE_DISTANCE)
                return true;
            else
                return false;
        }

        public void Update(GameTime gameTime)
        {
            var iterator = _objectsList.First;
            while (iterator != null)
            {
                iterator.Value.Update(gameTime);

                if (Outside(iterator.Value))
                {
                    var removerIterator = iterator;
                    iterator = iterator.Next;
                    _objectsList.Remove(removerIterator);
                }
                else
                {
                    iterator = iterator.Next;
                }
            }
            HandleBackgroundSpawning(gameTime);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {
            foreach (var item in _objectsList)
            {
                item.Draw(batch);
            }
        }
    }
}
