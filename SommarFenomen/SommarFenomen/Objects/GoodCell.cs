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

        private List<Virus> _virusList = new List<Virus>();
        private int _virusResistance;

        public GoodCell(PlayWindow playWindow) : base(playWindow, new StationaryStrategy())
        {
            Init();
        }

        public GoodCell(PlayWindow playWindow, Vector2 position)
            : base(playWindow, new StationaryStrategy())
        {
            Position = position;
            Init();
        }

        private void Init()
        {
            Sprite = new Sprite(_cellTexture);
            Sprite.CenterOrigin();
            Sprite.Scale = new Vector2(0.5f);
            _virusResistance = 5;
            CreateBody();
            Body.LinearDamping = 1;
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
                watch.Restart();
            }
        }

        public override void CreateBody()
        {
            Body = BodyFactory.CreateCircle(PlayWindow.World, ConvertUnits.ToSimUnits(Sprite.ScaledSize.X / 2), 1);
            Body.BodyType = BodyType.Dynamic;
            Body.Position = ConvertUnits.ToSimUnits(Position);
            Body.UserData = this;
        }

        private void ChangeSize(float percentage)
        {
            float oldRadius = Body.FixtureList.First().Shape.Radius;
            Body.DestroyFixture(Body.FixtureList.First());
            FixtureFactory.AttachCircle(oldRadius * percentage, 1, Body);
            Sprite.Scale *= percentage;
        }

        public bool VirusCollide(Virus virus)
        {
            _virusResistance--;
            if (_virusResistance == 0)
            {
                _virusList.Add(virus);
                return true;
            }
            else
                return false;
        }

        public void ExplodeByVirus()
        {
            float radius = Body.FixtureList.First().Shape.Radius;
            for (int i = 0; i < 9 + _virusList.Count; i++)
            {
                Vector2 position;
                position.X = this.Position.X - radius / 2 + (float)Shared.Random.NextDouble() * radius;
                position.Y = this.Position.Y - radius / 2 + (float)Shared.Random.NextDouble() * radius;

                Virus newVirus = new Virus(PlayWindow, position);
                PlayWindow.RegisterVirus(newVirus);

                ApplyExplodeForce(newVirus);
            }

            foreach (var item in _virusList)
            {
                PlayWindow.RemoveVirus(item);
            }
            PlayWindow.RemoveGoodCell(this);
        }

        private static readonly float FORCE_FACTOR = 1000;
        private void ApplyExplodeForce(Virus virus)
        {
            float radius = Body.FixtureList.First().Shape.Radius;
            Vector2 force, direction;
            force = direction = virus.Position - this.Position;

            direction.Normalize();

            force /= radius * 2;
            force += direction / 2;
            Console.WriteLine("Virus force " + force);
            virus.Body.ApplyForce(force * FORCE_FACTOR);
        }

        public override bool ObjectCollision(FarseerPhysics.Dynamics.Fixture f1, FarseerPhysics.Dynamics.Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }
    }
}
