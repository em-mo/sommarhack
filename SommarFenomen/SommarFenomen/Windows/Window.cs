using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SommarFenomen
{
    interface Window
    {
        void Initialize();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}
