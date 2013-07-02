using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects.Strategies;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using SommarFenomen.Util;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace SommarFenomen.Objects
{
    class Virus : ActiveGameObject
    {
        static private List<Texture2D> _virusTexture = new List<Texture2D>();
        int virusType;
        float[] _bodyOffsets = new float[2];

        GoodCell _targetCell;
        private bool _assimilatingCell;

        public Virus(PlayWindow playWindow) : base(playWindow, new VirusStrategy())
        {
            init();
        }

        public Virus(PlayWindow playWindow, Vector2 position)
            : base(playWindow, new VirusStrategy())
        {
            Position = position;
            init();

        }
        private void init()
        {
            Strategy.Owner = this;
            virusType = Shared.Random.Next(4);
            Sprite = new Sprite(_virusTexture[virusType]);
            Sprite.CenterOrigin();
            CreateBody();
            Body.UserData = this;
            _assimilatingCell = false;
            Body.OnCollision += ObjectCollision;

        }

        static public void LoadContent()
        {
            for (int i = 1; i <= 4; i++)
            {
                Texture2D texture = Game1.contentManager.Load<Texture2D>(@"Images\Virus" + i.ToString());
                _virusTexture.Add(texture);
            }
        }

        private static readonly float VIRUS_LOWER_CIRCLE_SCALE = 0.47f;
        private static readonly float VIRUS_UPPER_CIRCLE_SCALE = 0.33f;
        private static readonly float VIRUS_LOWER_CIRCLE_OFFSET = 0.10f;
        private static readonly float VIRUS_UPPER_CIRCLE_OFFSET = 0.20f;

        public override void CreateBody()
        {
            Body = BodyFactory.CreateBody(PlayWindow.World, ConvertUnits.ToSimUnits(Position));

            _bodyOffsets[0] = ConvertUnits.ToSimUnits(_virusTexture[virusType].Height * VIRUS_LOWER_CIRCLE_OFFSET);
            _bodyOffsets[1] = ConvertUnits.ToSimUnits(-_virusTexture[virusType].Height * VIRUS_UPPER_CIRCLE_OFFSET);

            switch (virusType)
            {
                case 0:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTexture[virusType].Width / 2), 1, Body);
                    break;
                case 1:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTexture[virusType].Width / 2), 1, Body);
                    break;
                case 2:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTexture[virusType].Width * VIRUS_LOWER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[0]));
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTexture[virusType].Width * VIRUS_UPPER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[1]));
                    break;
                case 3:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTexture[virusType].Width * VIRUS_LOWER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[0]));
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTexture[virusType].Width * VIRUS_UPPER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[1]));
                    break;
                default:
                break;
            }
            Body.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
            Body.LinearDamping = 1;
        }

        private void ChangeSize(float percentage)
        {
            if (Body.FixtureList.First().Shape.Radius > 0.01f)
                switch(virusType)
                {
                    case 0:
                        ChangeSizeSingle(percentage);
                        break;
                    case 1:
                        ChangeSizeSingle(percentage);
                        break;
                    case 2:
                        ChangeSizeDual(percentage);
                        break;
                    case 3:
                        ChangeSizeDual(percentage);
                        break;
                }
        }

        private void ChangeSizeSingle(float percentage)
        {
            float oldRadius = Body.FixtureList.First().Shape.Radius;
            Body.DestroyFixture(Body.FixtureList.First());
            FixtureFactory.AttachCircle(oldRadius * percentage, 1, Body);
            Sprite.Scale *= percentage;
        }

        private void ChangeSizeDual(float percentage)
        {
            float[] oldRadius = new float[2];
            float[] newRadius = new float[2];
            oldRadius[0] = Body.FixtureList[0].Shape.Radius;
            oldRadius[1] = Body.FixtureList[1].Shape.Radius;

            Body.DestroyFixture(Body.FixtureList.First());
            Body.DestroyFixture(Body.FixtureList.First());

            _bodyOffsets[0] *= percentage;
            _bodyOffsets[1] *= percentage;

            FixtureFactory.AttachCircle(oldRadius[0] * percentage, 1, Body, new Vector2(0, _bodyOffsets[0]));
            FixtureFactory.AttachCircle(oldRadius[1] * percentage, 1, Body, new Vector2(0, _bodyOffsets[1]));

            Sprite.Scale *= percentage;
        }

        public override bool ObjectCollision(FarseerPhysics.Dynamics.Fixture f1, FarseerPhysics.Dynamics.Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            Object o1, o2;
            o1 = f1.Body.UserData;
            o2 = f2.Body.UserData;
            GoodCell goodCell;
            if (o1 != null && o1 is GoodCell)
            {
                goodCell = (GoodCell)o1;
            }
            else if (o2 != null && o2 is GoodCell)
            {
                goodCell = (GoodCell)o2;
            }
            else 
                return true;

            if (goodCell.VirusCollide())
            {
                _assimilatingCell = true;
                Body.IgnoreCollisionWith(goodCell.Body);
                _targetCell = goodCell;
                Strategy = new VirusAssimilateStrategy(goodCell);
                Body.LinearDamping = 10;
            }

            return true;
        }

        private static readonly double SHRINK_TIME = 0.01;
        private static readonly float SHRINK_PERCENTAGE = 0.99f;
        private double _timer = 0;
        public override void Update(GameTime gameTime)
        {
            Console.WriteLine(Body.Mass);
            if (_assimilatingCell)
            {
                _timer += gameTime.ElapsedGameTime.TotalSeconds;

                if (_timer > SHRINK_TIME)
                {
                    ChangeSize(SHRINK_PERCENTAGE);
                    Body.IgnoreCollisionWith(_targetCell.Body);
                    Body.Mass = 2;
                    _timer = 0;
                }
            }

            base.Update(gameTime);
        }
    }
}
