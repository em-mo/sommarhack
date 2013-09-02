using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SommarFenomen.Objects;
using Microsoft.Xna.Framework;
using SommarFenomen.Util;

namespace SommarFenomen.Windows.WindowUtils
{
    class BackgroundObjectsHandler
    {
        private LinkedList<BackgroundObject> _objectsList = new LinkedList<BackgroundObject>();
        private Camera2D _camera;

        public BackgroundObjectsHandler(Camera2D camera)
        {
            _camera = camera;
        }



        public void Update(GameTime gameTime)
        {
            foreach (var item in _objectsList)
            {
                item.Update(gameTime);
            }
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
