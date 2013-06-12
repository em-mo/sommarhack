using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SommarFenomen
{
    interface IStrategy
    {
        Vector2 GetAcceleration();
    }
}
