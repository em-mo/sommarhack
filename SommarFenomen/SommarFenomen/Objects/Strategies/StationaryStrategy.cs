using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SommarFenomen.Objects.Strategies
{
    class StationaryStrategy : Strategy
    {
        public override Microsoft.Xna.Framework.Vector2 GetAcceleration()
        {
            return Microsoft.Xna.Framework.Vector2.Zero;
        }
    }
}
