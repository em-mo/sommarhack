using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects.Strategies;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using SommarFenomen.Util;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;

namespace SommarFenomen.Objects
{
    class Virus : ActiveGameObject
    {
        static private List<Texture2D> _bodyTextures = new List<Texture2D>();
        static private List<Texture2D> _mouthTextures = new List<Texture2D>();
        static private List<Texture2D> _eyeTextures = new List<Texture2D>();
        static private List<Texture2D> _virusTextures = new List<Texture2D>();
        static private List<Texture2D> _virusTextureCombinations = new List<Texture2D>();
        private Texture2D _virusTexture;
        private int _virusType;
        private float[] _bodyOffsets = new float[2];

        private DistanceJoint _playerBodyJoint;

        private GoodCell _targetCell;

        private enum VirusCellState
        {
            Normal,
            Grabbed,
            SuckedIn,
            Consumed,
            Assimilating
        }
        private VirusCellState _state = VirusCellState.Normal;

        public bool IsNormal() { return _state == VirusCellState.Normal; }
        public bool IsConsumed() { return _state == VirusCellState.Consumed; }
        public bool IsGrabbed() { return _state == VirusCellState.Grabbed; }
        public bool IsEntering() { return _state == VirusCellState.SuckedIn; }
        public bool IsAssimilating() { return _state == VirusCellState.Assimilating; }

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
            _virusType = Shared.Random.Next(4);
            _virusType = 0;
            InitCellTextures();
            Sprite = new Sprite(_virusTexture);
            Sprite.CenterOrigin();
            Sprite.Scale = new Vector2(0.08f);
            CreateBody();
            Body.UserData = this;
        }

        private void OnChangeSettings()
        {
            Body.OnCollision += ObjectCollision;
            Body.Mass = 2.2f;
        }

        private void InitCellTextures()
        {
            _virusTexture = _virusTextureCombinations[Shared.Random.Next(_virusTextureCombinations.Count())];
        }

        static public void LoadContent(GraphicsDevice graphicsDevice)
        {
            for (int i = 1; i <= 4; i++)
            {
                Texture2D texture = Game1.contentManager.Load<Texture2D>(@"Images\Virus" + i.ToString());
                _virusTextures.Add(texture);
            }

            string loadString = @"Images\Characters\Ond\B_body_";
            for (int i = 1; i <= 6; i++)
            {
                _bodyTextures.Add(Game1.contentManager.Load<Texture2D>(loadString + i));
            }

            loadString = @"Images\Characters\Ond\B_mouth_";
            for (int i = 1; i <= 5; i++)
            {
                _mouthTextures.Add(Game1.contentManager.Load<Texture2D>(loadString + i));
            }

            loadString = @"Images\Characters\Ond\B_eye_";
            for (int i = 1; i <= 2; i++)
            {
                _eyeTextures.Add(Game1.contentManager.Load<Texture2D>(loadString + i));
            }
            CombineAllTextures(graphicsDevice);
        }

        static public void CombineAllTextures(GraphicsDevice graphicsDevice)
        {
            for (int bodyCounter = 0; bodyCounter < _bodyTextures.Count(); bodyCounter++)
            {
                for (int eyeCounter = 0; eyeCounter < _eyeTextures.Count(); eyeCounter++)
                {
                    for (int mouthCounter = 0; mouthCounter < _mouthTextures.Count(); mouthCounter++)
                    {
                        Texture2D combinedTexture;
                        combinedTexture = Utils.MergeTextures(_bodyTextures[bodyCounter], _eyeTextures[eyeCounter], graphicsDevice);
                        combinedTexture = Utils.MergeTextures(combinedTexture, _mouthTextures[mouthCounter], graphicsDevice);
                        _virusTextureCombinations.Add(combinedTexture);
                    }
                }
            }
        }

        private static readonly float VIRUS_LOWER_CIRCLE_SCALE = 0.47f;
        private static readonly float VIRUS_UPPER_CIRCLE_SCALE = 0.33f;
        private static readonly float VIRUS_LOWER_CIRCLE_OFFSET = 0.10f;
        private static readonly float VIRUS_UPPER_CIRCLE_OFFSET = 0.20f;

        public override void CreateBody()
        {
            Body = BodyFactory.CreateBody(PlayWindow.World, ConvertUnits.ToSimUnits(Position));

            _bodyOffsets[0] = ConvertUnits.ToSimUnits(_virusTextures[_virusType].Height * VIRUS_LOWER_CIRCLE_OFFSET);
            _bodyOffsets[1] = ConvertUnits.ToSimUnits(-_virusTextures[_virusType].Height * VIRUS_UPPER_CIRCLE_OFFSET);

            switch (_virusType)
            {
                case 0:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(Sprite.ScaledSize.X / 2), 1, Body);
                    break;
                case 1:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTextures[_virusType].Width / 2), 1, Body);
                    break;
                case 2:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTextures[_virusType].Width * VIRUS_LOWER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[0]));
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTextures[_virusType].Width * VIRUS_UPPER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[1]));
                    break;
                case 3:
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTextures[_virusType].Width * VIRUS_LOWER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[0]));
                    FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(_virusTextures[_virusType].Width * VIRUS_UPPER_CIRCLE_SCALE), 1, Body, new Vector2(0, _bodyOffsets[1]));
                    break;
                default:
                break;
            }
            Body.CollisionCategories = Category.Cat5;
            Body.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
            Body.OnCollision += ObjectCollision;
            Body.Mass = 2.2f;
            Body.LinearDamping = 1;
            Body.UserData = this;
        }

        public void Consumed()
        {
            _state = VirusCellState.Consumed;
            Body.CollidesWith = Category.All & ~Category.Cat5;
            RemovePlayerJoint();
        }


        public void Grabbed()
        {
            _state = VirusCellState.Grabbed;
            Body.Mass = 0.1f;
        }
        

        public void EnteringPlayerCell(Body playerBody)
        {
            _state = VirusCellState.SuckedIn;
            Strategy = new StationaryStrategy();
            Body.CollidesWith = Category.All & ~Category.Cat10 & ~Category.Cat5;
            Body.Mass = 1;
            CreatePlayerJoint(playerBody);
        }

        private void CreatePlayerJoint(Body playerBody)
        {
            _playerBodyJoint = JointFactory.CreateDistanceJoint(PlayWindow.World, Body, playerBody, Vector2.Zero, Vector2.Zero);
            _playerBodyJoint.Frequency = 5.5f;
            _playerBodyJoint.DampingRatio = 1.5f;
            _playerBodyJoint.Length = 0.01f;
            _playerBodyJoint.CollideConnected = true;
        }

        private void RemovePlayerJoint()
        {
            if (_playerBodyJoint != null)
            {
                PlayWindow.World.RemoveJoint(_playerBodyJoint);
                _playerBodyJoint = null;
            }
        }

        public void Dropped()
        {
            if (IsGrabbed())
            {
                _state = VirusCellState.Normal;
                Body.Mass = 2.2f;
                Body.CollidesWith = Category.All;
            }
        }

        private void ChangeSize(float percentage)
        {
            if (Body.FixtureList.First().Shape.Radius > 0.01f)
                switch(_virusType)
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
            OnChangeSettings();
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
            if (o2 is GoodCell)
                goodCell = (GoodCell)o2;
            else if (o2 is PlayerCell)
                return false;
            else
                return true;

            // If cell resistance is low enough, enter the cell and go towards the center
            if (IsNormal() && goodCell.VirusCollide(this))
            {
                _state = VirusCellState.Assimilating;
                Body.IgnoreCollisionWith(goodCell.Body);
                _targetCell = goodCell;
                Strategy = new VirusAssimilateStrategy(goodCell);
                Body.LinearDamping = 10;
            }

            return true;
        }

        private static readonly double FADE_OUT_TIME = 6;
        private static readonly double FADE_STEP_TIME = FADE_OUT_TIME / 256;
        private double _fadeStepTimer = 0;
        private double _fadeOutTimer = 0;
        private void FadeOut(GameTime gameTime)
        {
            _fadeStepTimer += gameTime.ElapsedGameTime.TotalSeconds;
            _fadeOutTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (_fadeOutTimer > FADE_OUT_TIME)
            {
                PlayWindow.RemoveVirus(this);
            }
            while (_fadeStepTimer > FADE_STEP_TIME)
            {
                Color color = Sprite.Color;
                color.A -= 1;
                color.R -= 1;
                color.G -= 1;
                color.B -= 1;
                Sprite.Color = color;
                _fadeStepTimer -= FADE_STEP_TIME;
            }
        }

        private static readonly double SHRINK_TIME = 0.015;
        private static readonly double EXPLODE_TIME = 2;
        private static readonly float SHRINK_PERCENTAGE = 0.99f;
        private double _shrinkTimer = 0;
        private double _explodeTimer = 0;
        public override void Update(GameTime gameTime)
        {
            if (IsAssimilating())
            {
                _shrinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
                _explodeTimer += gameTime.ElapsedGameTime.TotalSeconds;
                while (_shrinkTimer > SHRINK_TIME)
                {
                    ChangeSize(SHRINK_PERCENTAGE);
                    Body.IgnoreCollisionWith(_targetCell.Body);
                    Body.Mass = 2;
                    _shrinkTimer -= SHRINK_TIME;
                }

                if (_explodeTimer > EXPLODE_TIME)
                {
                    _targetCell.ExplodeByVirus();
                }
            }
            else if (IsConsumed())
            {
                FadeOut(gameTime);
            }

            base.Update(gameTime);
        }
    }
}
