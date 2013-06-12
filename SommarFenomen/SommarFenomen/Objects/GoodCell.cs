using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SommarFenomen.Objects.Strategies;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SommarFenomen.Objects
{
    class GoodCell : ActiveGameObject
    {
        static Texture2D cellTexture;

        Sprite cellSprite;
        public GoodCell() : base(new StationaryStrategy())
        {
            cellSprite = new Sprite(cellTexture);
        }

        public static void LoadContent()
        {
            cellTexture = Game1.contentManager.Load<Texture2D>(@"Images\Good_Cell");
        }

        public override void Draw(SpriteBatch batch)
        {
            GraphicsHandler.DrawSprite(cellSprite, batch);
        }
    }
}
