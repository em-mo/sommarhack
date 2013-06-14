using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects.Strategies;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using SommarFenomen.Util;

namespace SommarFenomen.Objects
{
    class Virus : ActiveGameObject
    {
        static private List<Texture2D> _virusTexture = new List<Texture2D>();
        int virusType;
        public Virus(PlayWindow playWindow) : base(playWindow, new VirusStrategy())
        {
            virusType = Shared.Random.Next(4);
            Sprite = new Sprite(_virusTexture[virusType]);
            Sprite.CenterOrigin();
        }

        public Virus(PlayWindow playWindow, Vector2 position)
            : base(playWindow, new VirusStrategy())
        {
            virusType = Shared.Random.Next(4);
            Position = position;
            Sprite = new Sprite(_virusTexture[virusType]);
            Sprite.CenterOrigin();
            CreateBody();
        }

        static public void LoadContent()
        {
            for (int i = 1; i <= 4; i++)
            {
                Texture2D texture = Game1.contentManager.Load<Texture2D>(@"Images\Virus" + i.ToString());
                _virusTexture.Add(texture);
            }
        }

        public override void CreateBody()
        {
            Body = BodyFactory.CreateBody(PlayWindow.World, ConvertUnits.ToSimUnits(Position));
            FixtureFactory.AttachCircle(1, 1, Body, new Vector2(0, 1.5f));
            FixtureFactory.AttachCircle(1, 1, Body, new Vector2(0, -1.5f));

            Body.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
        }

        public override bool ObjectCollision(FarseerPhysics.Dynamics.Fixture f1, FarseerPhysics.Dynamics.Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }
    }
}
