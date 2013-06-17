using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SommarFenomen.Objects;

namespace SommarFenomen
{
    class Strategy
    {
        public ActiveGameObject Owner { get; set; }

        public Strategy()
        {
            Owner = null;
        }

        public abstract Vector2 GetAcceleration();
        public virtual void Update(GameTime gameTime) { }
    }
}
