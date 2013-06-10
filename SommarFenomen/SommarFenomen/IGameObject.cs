using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SommarFenomen
{
    interface IGameObject
    {
        public void Update(GameTime gameTime);
        public void Draw(GraphicsHandler g);
    }
}
