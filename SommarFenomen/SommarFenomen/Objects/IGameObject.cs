using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen
{
    interface IGameObject
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch batch);
    }
}
