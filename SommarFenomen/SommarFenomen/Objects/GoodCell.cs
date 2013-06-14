using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SommarFenomen.Objects.Strategies;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Factories;
using SommarFenomen.Util;
using FarseerPhysics.Dynamics;
using System.Diagnostics;

namespace SommarFenomen.Objects
{
    class GoodCell : ActiveGameObject
    {
        static private Texture2D _cellTexture;
        private Stopwatch watch = new Stopwatch();

        public GoodCell(PlayWindow playWindow) : base(playWindow, new StationaryStrategy())
        {
            Sprite = new Sprite(_cellTexture);
            Sprite.CenterOrigin();
            CreateBody();
        }

        public GoodCell(PlayWindow playWindow, Vector2 position)
            : base(playWindow, new StationaryStrategy())
        {
            Sprite = new Sprite(_cellTexture);
            Sprite.CenterOrigin();
            Sprite.Scale = new Vector2(0.5f);
            Position = position;
            CreateBody();
            watch.Start();
        }

        public static void LoadContent()
        {
            _cellTexture = Game1.contentManager.Load<Texture2D>(@"Images\Good_Cell");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (watch.ElapsedMilliseconds > 3000)
            {
                ChangeSize(1.05f);
                watch.Restart();
            }
        }

        public override void CreateBody()
        {
            Body = BodyFactory.CreateCircle(PlayWindow.World, ConvertUnits.ToSimUnits(Sprite.ScaledSize.X / 2), 1);
            Body.BodyType = BodyType.Dynamic;
            Body.Position = ConvertUnits.ToSimUnits(Position);
        }

        private void ChangeSize(float percentage)
        {
            float oldRadius = Body.FixtureList.First().Shape.Radius;
            Body.DestroyFixture(Body.FixtureList.First());
            FixtureFactory.AttachCircle(oldRadius * percentage, 1, Body);
            Sprite.Scale *= percentage;
        }

        public override bool ObjectCollision(FarseerPhysics.Dynamics.Fixture f1, FarseerPhysics.Dynamics.Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }
    }
}
