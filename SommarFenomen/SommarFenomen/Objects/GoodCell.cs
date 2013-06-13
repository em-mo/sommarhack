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
        static private Texture2D _cellTexture;

        private Sprite _cellSprite;
        public GoodCell(PlayWindow playWindow) : base(playWindow, new StationaryStrategy())
        {
            _cellSprite = new Sprite(_cellTexture);
        }

        public static void LoadContent()
        {
            _cellTexture = Game1.contentManager.Load<Texture2D>(@"Images\Good_Cell");
        }

        public override void Draw(SpriteBatch batch)
        {
            GraphicsHandler.DrawSprite(_cellSprite, batch);
        }

        public override void CreateBody()
        {
            return;
        }

        public override bool ObjectCollision(FarseerPhysics.Dynamics.Fixture f1, FarseerPhysics.Dynamics.Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }
    }
}
