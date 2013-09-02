using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SommarFenomen.Objects.Strategies
{
    class StraightStrategy : Strategy
    {
        public Vector2 Velocity { get; set; }

        public StraightStrategy(Vector2 velocity)
        {
            Velocity = velocity;
        }

        public override Vector2 GetAcceleration()
        {
            return Velocity;
        }
    }
}
