using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using SommarFenomen.Util;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;

namespace SommarFenomen.Objects
{
    class PlayerCell : ActiveGameObject
    {
        enum PlayerSprites { Cell, LeftHumerus, LeftUlna, LeftHand, RightHumerus, RightUlna, RightHand };

        private Texture2D _cellTexture;

        private Dictionary<Direction, Texture2D> _cloudTextures;
        private Sprite _windPuff;

        private static readonly float DRAG_ACCELERATION = -150;
        private static readonly double MAX_SPEED = 500;
        private static readonly float ARM_SCALE = 0.75f;
        private static float DIRECTION_SPRITE_THRESHOLD = 100;

        private float _rightHumerusOffsetX;
        private float _rightHumerusOffsetY;
        private float _rightUlnaOffset;
        private float _rightHandOffset;
        private float _leftHumerusOffsetX;
        private float _leftHumerusOffsetY;
        private float _leftUlnaOffset;
        private float _leftHandOffset;

        private Vector2 _origin;

        private List<WindPuffMessage> _windPuffList = new List<WindPuffMessage>();

        private Dictionary<PlayerSprites, Sprite> _spriteDict;

        public readonly object locker = new object();

        //Sets position of all sprites
        private void PositionHelper(Vector2 v)
        {
            Vector2 diffVector = v - _spriteDict[PlayerSprites.Cell].Position;

            foreach (Sprite sprite in _spriteDict.Values)
                Utils.AddToSpritePosition(sprite, diffVector);

            _spriteDict[PlayerSprites.Cell].Position = v;
        }

        public PlayerCell(PlayWindow playWindow, Vector2 position) : base(playWindow, new KinectStrategy(), MAX_SPEED)
        {
            _cellTexture = Game1.contentManager.Load<Texture2D>(@"Images\Hero_Cell");
            _spriteDict = new Dictionary<PlayerSprites, Sprite>();
            InitSprites();
            Position = position;
            _spriteDict[PlayerSprites.Cell].Position = Position;
            InitArms();
            CreateBody();
            CreateSoftBody(20, 200, 30, 1, 0.8f, 1.0f);

            SetLeftArmRotation((float)Math.PI / 2, (float)Math.PI / 2);
            SetRightArmRotation(-(float)Math.PI / 2, -(float)Math.PI / 2);
        }

        private void InitSprites()
        {
            const float CLOUD_SCALE = 0.6F;
            const float ARM_SCALE = 1f;

            _spriteDict = new Dictionary<PlayerSprites, Sprite>();

            foreach (PlayerSprites sprite in Enum.GetValues(typeof(PlayerSprites)))
            {
                _spriteDict.Add(sprite, new Sprite());
            }
            _windPuff = new Sprite();

            _cloudTextures = new Dictionary<Direction, Texture2D>();
            _cloudTextures.Add(Direction.None, Game1.contentManager.Load<Texture2D>(@"Images\Cloud"));
            _cloudTextures.Add(Direction.Left, Game1.contentManager.Load<Texture2D>(@"Images\Cloud_Move_Left"));
            _cloudTextures.Add(Direction.Right, Game1.contentManager.Load<Texture2D>(@"Images\Cloud_Move_Right"));

            _windPuff.Texture = Game1.contentManager.Load<Texture2D>(@"Images\wind");
            _spriteDict[PlayerSprites.Cell].Texture = _cellTexture;
            _spriteDict[PlayerSprites.LeftHumerus].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Humerus_left");
            _spriteDict[PlayerSprites.LeftHand].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Hand_left");
            _spriteDict[PlayerSprites.RightHumerus].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Humerus_right");
            _spriteDict[PlayerSprites.RightHand].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Hand_right");
            _spriteDict[PlayerSprites.LeftUlna].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Ulna_left");
            _spriteDict[PlayerSprites.RightUlna].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Ulna_right");

            _spriteDict[PlayerSprites.Cell].Scale = Vector2.One * CLOUD_SCALE;


            //Scale Left
            _spriteDict[PlayerSprites.LeftHumerus].Scale = Vector2.One * ARM_SCALE;
            _spriteDict[PlayerSprites.LeftUlna].Scale = Vector2.One * ARM_SCALE;
            _spriteDict[PlayerSprites.LeftHand].Scale = Vector2.One * ARM_SCALE;

            // Origin to right mid
            _spriteDict[PlayerSprites.LeftHumerus].Origin = new Vector2(_spriteDict[PlayerSprites.LeftHumerus].OriginalSize.X, _spriteDict[PlayerSprites.LeftHumerus].OriginalSize.Y / 2);
            _spriteDict[PlayerSprites.LeftUlna].Origin = new Vector2(_spriteDict[PlayerSprites.LeftUlna].OriginalSize.X, _spriteDict[PlayerSprites.LeftUlna].OriginalSize.Y / 2);
            _spriteDict[PlayerSprites.LeftHand].Origin = new Vector2(_spriteDict[PlayerSprites.LeftHand].OriginalSize.X, _spriteDict[PlayerSprites.LeftHand].OriginalSize.Y * 5 / 7);

            //Scale Right
            _spriteDict[PlayerSprites.RightHumerus].Scale = Vector2.One * ARM_SCALE;
            _spriteDict[PlayerSprites.RightUlna].Scale = Vector2.One * ARM_SCALE;
            _spriteDict[PlayerSprites.RightHand].Scale = Vector2.One * ARM_SCALE;

            //Origin to left mid
            _spriteDict[PlayerSprites.RightHumerus].Origin = new Vector2(0, _spriteDict[PlayerSprites.RightHumerus].OriginalSize.Y / 2);
            _spriteDict[PlayerSprites.RightUlna].Origin = new Vector2(0, _spriteDict[PlayerSprites.RightUlna].OriginalSize.Y / 2);
            _spriteDict[PlayerSprites.RightHand].Origin = new Vector2(0, _spriteDict[PlayerSprites.RightHand].OriginalSize.Y * 5 / 7);
            
            //Origin center
            _windPuff.Origin = new Vector2(_windPuff.OriginalSize.X / 2, _windPuff.OriginalSize.Y / 2);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            PositionHelper(Position);
        }

        private void InitArms()
        {
            _leftHumerusOffsetX = (float)(_spriteDict[PlayerSprites.Cell].ScaledSize.X * 0.1);
            _leftHumerusOffsetY = (float)(_spriteDict[PlayerSprites.Cell].ScaledSize.Y * 0.6);
            _leftUlnaOffset = -(float)(_spriteDict[PlayerSprites.LeftHumerus].ScaledSize.X * 0.95);
            _leftHandOffset = -(float)(_spriteDict[PlayerSprites.LeftUlna].ScaledSize.X * 0.97);

            _rightHumerusOffsetX = (float)(_spriteDict[PlayerSprites.Cell].ScaledSize.X * 0.8);
            _rightHumerusOffsetY = (float)(_spriteDict[PlayerSprites.Cell].ScaledSize.Y * 0.6);
            _rightUlnaOffset = (float)(_spriteDict[PlayerSprites.RightHumerus].ScaledSize.X * 0.95);
            _rightHandOffset = (float)(_spriteDict[PlayerSprites.RightUlna].ScaledSize.X * 0.97);

            //Set left
            Vector2 newHumerusPosition = new Vector2();
            newHumerusPosition.X = Position.X + _leftHumerusOffsetX;
            newHumerusPosition.Y = Position.Y + _leftHumerusOffsetY;

            Vector2 newUlnaPosition = new Vector2();
            newUlnaPosition.X = newHumerusPosition.X + _leftUlnaOffset;
            newUlnaPosition.Y = newHumerusPosition.Y;

            Vector2 newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + _leftHandOffset;
            newHandPosition.Y = newUlnaPosition.Y;

            _spriteDict[PlayerSprites.LeftHumerus].Position = newHumerusPosition;
            _spriteDict[PlayerSprites.LeftUlna].Position = newUlnaPosition;
            _spriteDict[PlayerSprites.LeftHand].Position = newHandPosition;

            //Set right
            newHumerusPosition = new Vector2();
            newHumerusPosition.X = Position.X + _rightHumerusOffsetX;
            newHumerusPosition.Y = Position.Y + _rightHumerusOffsetY;

            newUlnaPosition = new Vector2();
            newUlnaPosition.X = newHumerusPosition.X + _rightUlnaOffset;
            newUlnaPosition.Y = newHumerusPosition.Y;

            newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + _rightHandOffset;
            newHandPosition.Y = newUlnaPosition.Y;

            _spriteDict[PlayerSprites.RightHumerus].Position = newHumerusPosition;
            _spriteDict[PlayerSprites.RightUlna].Position = newUlnaPosition;
            _spriteDict[PlayerSprites.RightHand].Position = newHandPosition;

        }

        /// <summary>
        /// Rotates all three sections of the arm
        /// </summary>
        /// <param name="humerusRotation">Clockwise rotation in radians</param>
        /// <param name="ulnaRotation">Clockwise rotation in radians</param>
        public void SetLeftArmRotation(float humerusRotation, float ulnaRotation)
        {

            _spriteDict[PlayerSprites.LeftHumerus].Rotation = humerusRotation;

            Vector2 newUlnaPosition = new Vector2();
            newUlnaPosition.X = _spriteDict[PlayerSprites.LeftHumerus].Position.X + (float)(Math.Cos(humerusRotation) * _leftUlnaOffset);
            newUlnaPosition.Y = _spriteDict[PlayerSprites.LeftHumerus].Position.Y + (float)(Math.Sin(humerusRotation) * _leftUlnaOffset);

            Vector2 newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + (float)(Math.Cos(ulnaRotation) * _leftHandOffset);
            newHandPosition.Y = newUlnaPosition.Y + (float)(Math.Sin(ulnaRotation) * _leftHandOffset);

            lock (locker)
            {
                _spriteDict[PlayerSprites.LeftUlna].Position = newUlnaPosition;
                _spriteDict[PlayerSprites.LeftUlna].Rotation = ulnaRotation;

                _spriteDict[PlayerSprites.LeftHand].Position = newHandPosition;
                _spriteDict[PlayerSprites.LeftHand].Rotation = ulnaRotation;
            }
        }

        /// <summary>
        /// Rotates all three sections of the arm
        /// </summary>
        /// <param name="humerusRotation">Clockwise rotation in radians</param>
        /// <param name="ulnaRotation">Clockwise rotation in radians</param>
        public void SetRightArmRotation(float humerusRotation, float ulnaRotation)
        {

            _spriteDict[PlayerSprites.RightHumerus].Rotation = humerusRotation;

            Vector2 newUlnaPosition = new Vector2();
            newUlnaPosition.X = _spriteDict[PlayerSprites.RightHumerus].Position.X + (float)(Math.Cos(humerusRotation) * _rightUlnaOffset);
            newUlnaPosition.Y = _spriteDict[PlayerSprites.RightHumerus].Position.Y + (float)(Math.Sin(humerusRotation) * _rightUlnaOffset);

            Vector2 newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + (float)(Math.Cos(ulnaRotation) * _rightHandOffset);
            newHandPosition.Y = newUlnaPosition.Y + (float)(Math.Sin(ulnaRotation) * _rightHandOffset);

            lock (locker)
            {
                _spriteDict[PlayerSprites.RightUlna].Position = newUlnaPosition;
                _spriteDict[PlayerSprites.RightUlna].Rotation = ulnaRotation;

                _spriteDict[PlayerSprites.RightHand].Position = newHandPosition;
                _spriteDict[PlayerSprites.RightHand].Rotation = ulnaRotation;
            }
        }

        /// <summary>
        /// Adds windpuff at selected hand rotated after the direction parameter
        /// </summary>
        /// <param name="rotation">The rotation of the puff in radians</param>
        /// <param name="arm">Arm target for wind puff</param>
        public void AddWindPuff(float rotation, Arm arm)
        {
            Sprite hand;
            float offset;
            if (arm == Arm.Left)
            {
                hand = _spriteDict[PlayerSprites.LeftHand];
                offset = -hand.OriginalSize.X;
            }
            else
            {
                hand = _spriteDict[PlayerSprites.RightHand];
                offset = hand.OriginalSize.X;
            }

            Vector2 position = new Vector2(hand.Position.X + offset, hand.Position.Y);

            _windPuffList.Add(new WindPuffMessage(rotation, position));
        }

        private void DrawWindPuff(SpriteBatch batch)
        {
            WindPuffMessage puff;
            for (int i = _windPuffList.Count - 1; i >= 0; i--)
            {
                puff = _windPuffList.ElementAt(i);

                _windPuff.Position = puff.Position;
                _windPuff.Rotation = puff.Direction;

                GraphicsHandler.DrawSprite(_windPuff, batch);

                if (puff.checkAge())
                    _windPuffList.RemoveAt(i);
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            DrawWindPuff(batch);
            foreach (Sprite sprite in _spriteDict.Values)
                GraphicsHandler.DrawSprite(sprite, batch);
        }

        public override void CreateBody()
        {
            uint[] data = new uint[_cellTexture.Width * _cellTexture.Height];
            
            _cellTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _cellTexture.Width, false);

            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);

            //2. To draw the texture the correct place.
            _origin = -centroid;
            _spriteDict[PlayerSprites.Cell].Origin = _origin;

            float scale = _spriteDict[PlayerSprites.Cell].Scale.X;

            foreach (PlayerSprites sprite in Enum.GetValues(typeof(PlayerSprites)))
            {
                if (sprite == PlayerSprites.Cell)
                    continue;
                else
                {
                    _spriteDict[sprite].Position -= _origin * scale;
                }
                
                
            }

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 4f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = EarclipDecomposer.ConvexPartition(textureVertices);

            //scale the vertices from graphics space to sim space
            Vector2 vertScale = ConvertUnits.ToSimUnits(new Vector2(1)) * scale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            Body = BodyFactory.CreateCompoundPolygon(PlayWindow.World, list, 1f, BodyType.Dynamic);
            Body.Position = ConvertUnits.ToSimUnits(Position);
            Body.BodyType = BodyType.Dynamic;
            Body.CollisionCategories = Category.Cat10;
            Body.CollidesWith = Category.All;
            Body.OnCollision += ObjectCollision;
            Body.Friction = 0.1f;
            Body.FixedRotation = true;
            Body.Mass = 2f;
            Body.LinearDamping = 1;
        }

        List<Body> _outerBodies;
        Body centerBody;
        private void CreateSoftBody(int numberOfOuterBodies, float innerDistance, float radius, float density, float damping, float frequency)
        {
            innerDistance = ConvertUnits.ToSimUnits(innerDistance);
            radius = ConvertUnits.ToSimUnits(radius);
            _outerBodies = new List<Body>();
            centerBody = BodyFactory.CreateCircle(PlayWindow.World, radius * 2, density);
            centerBody.Position = ConvertUnits.ToSimUnits(Position);
            centerBody.BodyType = BodyType.Dynamic;
            double radianStep = Math.PI * 2 / numberOfOuterBodies;
            centerBody.LinearDamping = 1;
            centerBody.Mass = 0.2f;

            Vector2 tmp = centerBody.Position;
            tmp.Y -= 5;
            centerBody.Position = tmp;

            for (int i = 0; i < numberOfOuterBodies; i++)
            {
                double currentAngle = radianStep * i;
                Body body = BodyFactory.CreateCircle(PlayWindow.World, radius, density);

                Vector2 direction = new Vector2();
                direction.X = (float)Math.Cos(currentAngle);
                direction.Y = (float)Math.Sin(currentAngle);

                body.Position = direction * innerDistance + centerBody.Position;
                body.BodyType = BodyType.Dynamic;
                body.Mass = 0.4f;
                _outerBodies.Add(body);
            }

            for (int i = 0; i < numberOfOuterBodies; i++)
            {
                DistanceJoint joint;
                // Outer joint
                Vector2 thisJointPosition, nextJointPosition, direction;

                int next = (i + 1) % numberOfOuterBodies;

                direction = _outerBodies[next].Position - _outerBodies[i].Position;
                direction.Normalize();

                thisJointPosition = Vector2.Zero;
                nextJointPosition = -Vector2.Zero;

                joint = JointFactory.CreateDistanceJoint(PlayWindow.World, _outerBodies[i], _outerBodies[next], thisJointPosition, nextJointPosition);
                joint.DampingRatio = damping;
                joint.Frequency = frequency;
                joint.CollideConnected = true;

                // Middle joint
                Vector2 centerJointPosition;

                direction = centerBody.Position - _outerBodies[i].Position;
                direction.Normalize();

                thisJointPosition = -direction * radius;
                centerJointPosition = direction * radius * 2;

                joint = JointFactory.CreateDistanceJoint(PlayWindow.World, _outerBodies[i], centerBody, thisJointPosition, centerJointPosition);
                joint.DampingRatio = damping;
                joint.Frequency = frequency;
                joint.CollideConnected = true;

                thisJointPosition = direction * radius;
                centerJointPosition = -direction * radius * 2;

                joint = JointFactory.CreateDistanceJoint(PlayWindow.World, _outerBodies[i], centerBody, thisJointPosition, centerJointPosition);
                joint.DampingRatio = damping;
                joint.Frequency = frequency;
                joint.CollideConnected = true;
            }
            centerBody.FixedRotation = true;
            centerBody.AngularVelocity = 1;
        }


        public override bool ObjectCollision(Fixture f1, Fixture f2, Contact contact)
        {
            return true;
        }
    }
}

