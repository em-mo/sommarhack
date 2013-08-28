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
        static private List<Texture2D> _bodyTextures = new List<Texture2D>();
        static private List<Texture2D> _mouthTextures = new List<Texture2D>();
        static private List<Texture2D> _eyeTextures = new List<Texture2D>();
        private Texture2D _happyTexture;
        private Texture2D _sadTexture;
        
        private Stopwatch watch = new Stopwatch();

        private List<Virus> _virusList = new List<Virus>();
        private int _virusResistance;
        private bool _infected = false;
        private static readonly float FORCE_FACTOR = 1000;

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
            InitCellTextures();
            Sprite = new Sprite(_sadTexture);
            Sprite.CenterOrigin();
            Sprite.Scale = new Vector2(0.14f);
            _virusResistance = 5;
            CreateBody();
            Body.LinearDamping = 1;
        }



        private void InitCellTextures()
        {
            Texture2D body = _bodyTextures[Shared.Random.Next(6)];
            Texture2D eyes = _eyeTextures[Shared.Random.Next(3)];
            Texture2D mouth = _mouthTextures[Shared.Random.Next(3)];

            _happyTexture = Utils.MergeTextures(body, _eyeTextures.Last(), PlayWindow.GraphicsDevice);

            _happyTexture = Utils.MergeTextures(_happyTexture, _mouthTextures.Last(), PlayWindow.GraphicsDevice);
            
            _sadTexture = Utils.MergeTextures(body, eyes, PlayWindow.GraphicsDevice);

            _sadTexture = Utils.MergeTextures(_sadTexture, mouth, PlayWindow.GraphicsDevice);
            

        }

        /// <summary>
        /// Fulhack, hardcoded values
        /// </summary>
        public static void LoadContent()
        {
            _cellTexture = Game1.contentManager.Load<Texture2D>(@"Images\Good_Cell");
            string loadString = @"Images\Characters\God\G_body_";
            for (int i = 1; i <= 6; i++)
            {
                _bodyTextures.Add(Game1.contentManager.Load<Texture2D>(loadString + i));
            }

            loadString = @"Images\Characters\God\G_mouth_";
            for (int i = 1; i <= 4; i++)
            {
                _mouthTextures.Add(Game1.contentManager.Load<Texture2D>(loadString + i));
            }

            loadString = @"Images\Characters\God\G_eye_";
            for (int i = 1; i <= 4; i++)
            {
                _eyeTextures.Add(Game1.contentManager.Load<Texture2D>(loadString + i));
            }
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

        public bool IsInfected()
        {
            return _infected;
        }

        public bool VirusCollide(Virus virus)
        {
            _virusResistance--;
            if (_virusResistance == 0)
            {
                _infected = true;
                _virusList.Add(virus);
                return true;
            }
            else
                return false;
        }

        public void ExplodeByVirus()
        {
            float radius = Body.FixtureList.First().Shape.Radius;
            for (int i = 0; i < 1 + _virusList.Count; i++)
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

        private void ApplyExplodeForce(Virus virus)
        {
            float radius = Body.FixtureList.First().Shape.Radius;
            Vector2 force, direction;
            force = direction = virus.Position - this.Position;

            direction.Normalize();

            force /= radius * 2;
            force += direction / 2;
            virus.Body.ApplyForce(force * FORCE_FACTOR);
        }

        public void SetHappy()
        {
            Sprite.Texture = _happyTexture;
        }

        public void SetSad()
        {
            Sprite.Texture = _sadTexture;
        }

        public override bool ObjectCollision(FarseerPhysics.Dynamics.Fixture f1, FarseerPhysics.Dynamics.Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }
    }
}
