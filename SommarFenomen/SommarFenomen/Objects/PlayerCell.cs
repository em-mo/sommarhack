﻿using System;
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
using FarseerPhysics.Collision;
using System.Diagnostics;

namespace SommarFenomen.Objects
{
    class PlayerCell : ActiveGameObject
    {
        enum PlayerSprites { Cell, LeftHumerus, LeftUlna, LeftHand, RightHumerus, RightUlna, RightHand };

        private static Texture2D _cellTexture;
        private Dictionary<PlayerSprites, Sprite> _spriteDict;
        //Eyes open in 0, eyes closed in 1
        private static Texture2D[] _happyTexture = new Texture2D[2];
        private static Texture2D[] _focusedTexture = new Texture2D[2];
        private Texture2D[] _currentStateTexture;

        private static readonly double MAX_SPEED = 500;

        private Vector2 _rightHumerusOffset = Vector2.Zero;
        private Vector2 _rightUlnaOffset = Vector2.Zero;
        private Vector2 _rightHandOffset = Vector2.Zero;
        private Vector2 _leftHumerusOffset = Vector2.Zero;
        private Vector2 _leftUlnaOffset = Vector2.Zero;
        private Vector2 _leftHandOffset = Vector2.Zero;

        private float _bodyRadius;
        private List<Body> _outerBodies;
        private Body _centerBody;
        private BasicEffect _bodyEffect;

        private Vector2 _origin;

        private List<WindPuffMessage> _windPuffList = new List<WindPuffMessage>();

        private VirusGrabber _leftHandGrabber;
        private VirusGrabber _rightHandGrabber;

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
            InitPlayerSprite();
            _spriteDict = new Dictionary<PlayerSprites, Sprite>();
            InitSprites();
            Position = position;
            _spriteDict[PlayerSprites.Cell].Position = Position;
            InitArms();

            _bodyRadius = 70;
            CreateSoftBody(30, _bodyRadius, 6f, 1, 0.5f, 3.0f);

            _bodyEffect = new BasicEffect(playWindow.GraphicsDevice);
            //_bodyEffect.EnableDefaultLighting();
            _bodyEffect.TextureEnabled = true;
            _bodyEffect.Texture = _cellTexture;
            _bodyEffect.World = Matrix.Identity;

            SetLeftArmRotation((float)Math.PI / 2, (float)Math.PI / 2);
            SetRightArmRotation(-(float)Math.PI / 2, -(float)Math.PI / 2);

            _leftHandGrabber = new VirusGrabber(VirusGrabber.Hand.LEFT, _spriteDict[PlayerSprites.LeftHand], PlayWindow.World, _centerBody);
            _rightHandGrabber = new VirusGrabber(VirusGrabber.Hand.RIGHT, _spriteDict[PlayerSprites.RightHand], PlayWindow.World, _centerBody);
        }

        private static Texture2D _bodyTexture;
        private static Texture2D _openEyesTexture;
        private static Texture2D _closedEyesTexture;
        private static Texture2D _happyMouthTexture;
        private static Texture2D _focusedMouthTexture;
        private static Dictionary<PlayerSprites, Texture2D> _armTextures = new Dictionary<PlayerSprites, Texture2D>();

        public static void LoadContent(GraphicsDevice graphicsDevice)
        {
            _bodyTexture = Game1.contentManager.Load<Texture2D>(@"Images\Characters\Hjalte\H_body");
            _openEyesTexture = Game1.contentManager.Load<Texture2D>(@"Images\Characters\Hjalte\H_eye_o");
            _closedEyesTexture = Game1.contentManager.Load<Texture2D>(@"Images\Characters\Hjalte\H_eye_c");
            _happyMouthTexture = Game1.contentManager.Load<Texture2D>(@"Images\Characters\Hjalte\H_mouth_1");
            _focusedMouthTexture = Game1.contentManager.Load<Texture2D>(@"Images\Characters\Hjalte\H_mouth_2");

            _cellTexture = Utils.MergeTextures(_bodyTexture, _happyMouthTexture, graphicsDevice);
            _happyTexture[0] = Utils.MergeTextures(_cellTexture, _openEyesTexture, graphicsDevice);
            _happyTexture[1] = Utils.MergeTextures(_cellTexture, _closedEyesTexture, graphicsDevice);

            _cellTexture = Utils.MergeTextures(_bodyTexture, _focusedMouthTexture, graphicsDevice);
            _focusedTexture[0] = Utils.MergeTextures(_cellTexture, _openEyesTexture, graphicsDevice);
            _focusedTexture[1] = Utils.MergeTextures(_cellTexture, _closedEyesTexture, graphicsDevice);

            _armTextures[PlayerSprites.Cell] = _happyTexture[0];
            _armTextures[PlayerSprites.LeftHumerus]= Game1.contentManager.Load<Texture2D>(@"images\Characters\Hjalte\H_arm_1_l");
            _armTextures[PlayerSprites.LeftHand] = Game1.contentManager.Load<Texture2D>(@"images\Characters\Hjalte\H_hand_l");
            _armTextures[PlayerSprites.RightHumerus] = Game1.contentManager.Load<Texture2D>(@"images\Characters\Hjalte\H_arm_1_r");
            _armTextures[PlayerSprites.RightHand] = Game1.contentManager.Load<Texture2D>(@"images\Characters\Hjalte\H_hand_r");
            _armTextures[PlayerSprites.LeftUlna] = Game1.contentManager.Load<Texture2D>(@"images\Characters\Hjalte\H_arm_2_l");
            _armTextures[PlayerSprites.RightUlna] = Game1.contentManager.Load<Texture2D>(@"images\Characters\Hjalte\H_arm_2_r");
        }

        private void InitPlayerSprite()
        {
            _cellTexture = _happyTexture[0];
            _currentStateTexture = _happyTexture;
        }

        private void InitSprites()
        {
            const float CLOUD_SCALE = 0.6F;
            const float ARM_SCALE = 0.20f;

            _spriteDict = new Dictionary<PlayerSprites, Sprite>();

            foreach (PlayerSprites sprite in Enum.GetValues(typeof(PlayerSprites)))
            {
                _spriteDict.Add(sprite, new Sprite());
                _spriteDict[sprite].Texture = _armTextures[sprite];
            }

            _spriteDict[PlayerSprites.Cell].Scale = Vector2.One * CLOUD_SCALE;
            _spriteDict[PlayerSprites.Cell].IsShowing = false;


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
        }

        private void InitArms()
        {
            _leftHumerusOffset.X = -(_spriteDict[PlayerSprites.Cell].ScaledSize.X * 0.08f);
            _leftHumerusOffset.Y = (_spriteDict[PlayerSprites.Cell].ScaledSize.Y * 0.017f);
            _leftUlnaOffset.X = -(_spriteDict[PlayerSprites.LeftHumerus].ScaledSize.X * 0.85f);
            _leftHandOffset.X = -(_spriteDict[PlayerSprites.LeftUlna].ScaledSize.X * 0.87f);
            _leftHandOffset.Y = -(_spriteDict[PlayerSprites.LeftUlna].ScaledSize.Y * 0.18f);

            _rightHumerusOffset.X = (_spriteDict[PlayerSprites.Cell].ScaledSize.X * 0.08f);
            _rightHumerusOffset.Y = (_spriteDict[PlayerSprites.Cell].ScaledSize.Y * 0.017f);
            _rightUlnaOffset.X = (_spriteDict[PlayerSprites.RightHumerus].ScaledSize.X * 0.85f);
            _rightHandOffset.X = (_spriteDict[PlayerSprites.RightUlna].ScaledSize.X * 0.87f);
            _rightHandOffset.Y = -(_spriteDict[PlayerSprites.RightUlna].ScaledSize.Y * 0.18f);

            //Set left
            Vector2 newHumerusPosition = new Vector2();
            newHumerusPosition = Position + _leftHumerusOffset;

            Vector2 newUlnaPosition = new Vector2();
            newUlnaPosition = newHumerusPosition + _leftUlnaOffset;

            Vector2 newHandPosition = new Vector2();
            newHandPosition = newUlnaPosition + _leftHandOffset;

            _spriteDict[PlayerSprites.LeftHumerus].Position = newHumerusPosition;
            _spriteDict[PlayerSprites.LeftUlna].Position = newUlnaPosition;
            _spriteDict[PlayerSprites.LeftHand].Position = newHandPosition;

            //Set right
            newHumerusPosition = new Vector2();
            newHumerusPosition = Position + _rightHumerusOffset;

            newUlnaPosition = new Vector2();
            newUlnaPosition = newHumerusPosition + _rightUlnaOffset;

            newHandPosition = new Vector2();
            newHandPosition = newUlnaPosition + _rightHandOffset;

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
            newUlnaPosition.X = _spriteDict[PlayerSprites.LeftHumerus].Position.X + (float)(Math.Cos(humerusRotation) * _leftUlnaOffset.X);
            newUlnaPosition.Y = _spriteDict[PlayerSprites.LeftHumerus].Position.Y + (float)(Math.Sin(humerusRotation) * _leftUlnaOffset.X);

            Vector2 newHandPosition = new Vector2();
            float cos, sin;
            cos = (float)(Math.Cos(ulnaRotation));
            sin = (float)(Math.Sin(ulnaRotation));
            newHandPosition.X = newUlnaPosition.X + cos * _leftHandOffset.X;
            newHandPosition.Y = newUlnaPosition.Y + sin * _leftHandOffset.X;
            newHandPosition.X += sin * _leftHandOffset.Y;
            newHandPosition.Y += cos * _rightHandOffset.Y;

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
            newUlnaPosition.X = _spriteDict[PlayerSprites.RightHumerus].Position.X + (float)(Math.Cos(humerusRotation) * _rightUlnaOffset.X);
            newUlnaPosition.Y = _spriteDict[PlayerSprites.RightHumerus].Position.Y + (float)(Math.Sin(humerusRotation) * _rightUlnaOffset.X);

            Vector2 newHandPosition = new Vector2();
            float cos, sin;
            cos = (float)(Math.Cos(ulnaRotation));
            sin = (float)(Math.Sin(ulnaRotation));
            newHandPosition.X = newUlnaPosition.X + cos * _rightHandOffset.X;
            newHandPosition.Y = newUlnaPosition.Y + sin * _rightHandOffset.X;
            newHandPosition.X += sin * _rightHandOffset.Y;
            newHandPosition.Y += cos * _rightHandOffset.Y;


            lock (locker)
            {
                _spriteDict[PlayerSprites.RightUlna].Position = newUlnaPosition;
                _spriteDict[PlayerSprites.RightUlna].Rotation = ulnaRotation;

                _spriteDict[PlayerSprites.RightHand].Position = newHandPosition;
                _spriteDict[PlayerSprites.RightHand].Rotation = ulnaRotation;
            }
        }

        private void HandleVirusGrabbing()
        {
            _leftHandGrabber.HandCollisions();
            _rightHandGrabber.HandCollisions();
            _leftHandGrabber.HandleVirusSprings();
            _rightHandGrabber.HandleVirusSprings();
        }

        private bool blinking = false;
        private double blinkingTimer = BLINK_COOLDOWN;
        private static readonly double BLINK_DURATION = 0.1;
        private static readonly double BLINK_COOLDOWN = 6;
        private static readonly double BLINK_DIFF = 1;
        private void HandleBlinkState(GameTime gameTime)
        {
            blinkingTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (blinkingTimer < 0)
            {
                if (blinking)
                {
                    _cellTexture = _currentStateTexture[0];
                    double diff = Shared.Random.NextDouble() * BLINK_DIFF - BLINK_DIFF / 2;
                    blinkingTimer += BLINK_COOLDOWN + diff;
                    blinking = false;
                }
                else
                {
                    _cellTexture = _currentStateTexture[1];
                    blinkingTimer += BLINK_DURATION;
                    blinking = true;
                }
            }
        }

        /// <summary>
        /// Old relic before the soft time
        /// </summary>
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


        private void CreateSoftBody(int numberOfOuterBodies, float innerDistance, float radius, float density, float damping, float frequency)
        {
            innerDistance = ConvertUnits.ToSimUnits(innerDistance);
            radius = ConvertUnits.ToSimUnits(radius);
            _outerBodies = new List<Body>();
            _centerBody = BodyFactory.CreateCircle(PlayWindow.World, radius * 5, density);
            _centerBody.Position = ConvertUnits.ToSimUnits(Position);
            _centerBody.BodyType = BodyType.Dynamic;
            double radianStep = Math.PI * 2 / numberOfOuterBodies;
            _centerBody.LinearDamping = 2;
            _centerBody.Mass = 0.2f;
            _centerBody.OnCollision += CenterBodyObjectCollision;
            _centerBody.UserData = this;

            for (int i = 0; i < numberOfOuterBodies; i++)
            {
                double currentAngle = radianStep * i;
                Body body = BodyFactory.CreateCircle(PlayWindow.World, radius, density);

                Vector2 direction = new Vector2();
                direction.X = (float)Math.Cos(currentAngle);
                direction.Y = (float)Math.Sin(currentAngle);

                body.Position = direction * innerDistance + _centerBody.Position;
                body.BodyType = BodyType.Dynamic;
                body.Mass = 0.4f;
                body.LinearDamping = 1;
                body.CollisionCategories = Category.Cat10;
                body.OnCollision += OuterBodyObjectCollision;
                _outerBodies.Add(body);
            }

            for (int i = 0; i < numberOfOuterBodies; i++)
            {
                DistanceJoint joint;
                // Outer joint
                Vector2 thisJointPosition, nextJointPosition, direction;

                int next = (i + 1) % numberOfOuterBodies;

                // Left over if you want the spring anywhere else than in the center
                direction = _outerBodies[next].Position - _outerBodies[i].Position;
                direction.Normalize();

                thisJointPosition = Vector2.Zero;
                nextJointPosition = -Vector2.Zero;

                joint = JointFactory.CreateDistanceJoint(PlayWindow.World, _outerBodies[i], _outerBodies[next], thisJointPosition, nextJointPosition);
                joint.DampingRatio = damping;
                joint.Frequency = frequency * 4;
                joint.CollideConnected = true;

                // Middle joint
                Vector2 centerJointPosition;

                // Left over if you want the spring anywhere else than in the center
                direction = _centerBody.Position - _outerBodies[i].Position;
                direction.Normalize();

                thisJointPosition = Vector2.Zero;
                centerJointPosition = Vector2.Zero;

                joint = JointFactory.CreateDistanceJoint(PlayWindow.World, _outerBodies[i], _centerBody, thisJointPosition, centerJointPosition);
                joint.DampingRatio = damping;
                joint.Frequency = frequency * 0.7f;
                joint.CollideConnected = true;

                int thirdNext = (i + 4) % numberOfOuterBodies;

                joint = JointFactory.CreateDistanceJoint(PlayWindow.World, _outerBodies[i], _outerBodies[thirdNext], Vector2.Zero, Vector2.Zero);
                joint.DampingRatio = damping;
                joint.Frequency = frequency * 3f;
                joint.CollideConnected = true;
            }
            Body = _centerBody;
        }
        #region softbodyvertices
        private VertexPositionNormalTexture[] getSoftBodyVertices()
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[_outerBodies.Count * 3];
            Vector3 normal = new Vector3(0, 0, 1);
            Vector2 direction = _centerBody.Position - _outerBodies[0].Position;
            direction.Normalize();

            float radianStep = 2 * (float)Math.PI / _outerBodies.Count;
            float currentAngle = (float)Utils.CalculateAngle(new Vector2(-1, 0), direction);

            float radius = _outerBodies[0].FixtureList[0].Shape.Radius;

            for (int i = 0; i < _outerBodies.Count; i++)
            {
                int currentVertex = i * 3;
                int next = (i + 1) % _outerBodies.Count;

                // First corner
                direction = _centerBody.Position - _outerBodies[i].Position;
                direction.Normalize();

                Vector2 vertex = ConvertUnits.ToDisplayUnits(_outerBodies[i].Position - direction * radius);
                vertices[currentVertex].Position = new Vector3(vertex, 0);
                vertices[currentVertex].TextureCoordinate = new Vector2(0.5f + (float)Math.Cos(currentAngle) / 2, 0.5f + (float)Math.Sin(currentAngle) / 2);
                vertices[currentVertex].Normal = normal;
                currentAngle += radianStep;

                // Second corner
                direction = _centerBody.Position - _outerBodies[next].Position;
                direction.Normalize();

                vertex = ConvertUnits.ToDisplayUnits(_outerBodies[next].Position - direction * radius);
                vertices[currentVertex + 1].Position = new Vector3(vertex, 0);
                vertices[currentVertex + 1].TextureCoordinate = new Vector2(0.5f + (float)Math.Cos(currentAngle) / 2, 0.5f + (float)Math.Sin(currentAngle) / 2);
                vertices[currentVertex + 1].Normal = normal;

                // Center
                vertex = ConvertUnits.ToDisplayUnits(_centerBody.Position);
                vertices[currentVertex + 2].Position = new Vector3(vertex, 0);
                vertices[currentVertex + 2].TextureCoordinate = new Vector2(0.5f, 0.5f);
                vertices[currentVertex + 2].Normal = normal;
            }

            return vertices;
        }

        private VertexPositionColor[] getSoftBodyBlueVertices()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[_outerBodies.Count * 3];
            Vector2 direction = _centerBody.Position - _outerBodies[0].Position;
            direction.Normalize();

            float radianStep = 2 * (float)Math.PI / _outerBodies.Count;
            float currentAngle = (float)Utils.CalculateAngle(new Vector2(0, 1), direction);

            float radius = _outerBodies[0].FixtureList[0].Shape.Radius;

            for (int i = 0; i < _outerBodies.Count; i++)
            {
                int currentVertex = i * 3;
                int next = (i + 1) % _outerBodies.Count;

                // First corner
                direction = _centerBody.Position - _outerBodies[i].Position;
                direction.Normalize();

                Vector2 vertex = _outerBodies[i].Position - direction * radius;
                vertices[currentVertex].Position = new Vector3(ConvertUnits.ToDisplayUnits(vertex), 0);
                vertices[currentVertex].Color = Color.OrangeRed;
                currentAngle += radianStep;

                // Second corner
                direction = _centerBody.Position - _outerBodies[next].Position;
                direction.Normalize();

                vertex = _outerBodies[next].Position - direction * radius;
                vertices[currentVertex + 1].Position = new Vector3(ConvertUnits.ToDisplayUnits(vertex), 0);
                vertices[currentVertex + 1].Color = Color.OrangeRed;

                // Center
                vertex = _centerBody.Position;
                vertices[currentVertex + 2].Position = new Vector3(ConvertUnits.ToDisplayUnits(vertex), 0);
                vertices[currentVertex + 2].Color = Color.OrangeRed;
            }

            return vertices;
        }
#endregion
        public override bool ObjectCollision(Fixture f1, Fixture f2, Contact contact)
        {
            return true;
        }

        private void SpawnCenterJoint(Virus virus)
        {
            
        }



        public bool OuterBodyObjectCollision(Fixture f1, Fixture f2, Contact contact)
        {
            Object o1, o2;
            o1 = f1.Body.UserData;
            o2 = f2.Body.UserData;

            if (o2 == null)
                return true;

            // If it isn't a collision with the grabbed virus
            if (o2 == _leftHandGrabber.GrabbedVirus() )
            {
                _leftHandGrabber.OuterWallCollision();
            }
            else if (o2 == _rightHandGrabber.GrabbedVirus())
            {
                _rightHandGrabber.OuterWallCollision();
            }

            return true;
        }

        private static readonly float VIRUS_CENTER_FACTOR = 0.5f;
        public  bool CenterBodyObjectCollision(Fixture f1, Fixture f2, Contact contact)
        {
            Object o2;
            o2 = f2.Body.UserData;
            Virus virus = null;

            if (o2 is Virus)
                virus = (Virus)o2;
            else
                return true;

            Vector2 distanceVector = f1.Body.Position - f2.Body.Position;

            if (virus.IsConsumed() == false && (distanceVector.Length() < ConvertUnits.ToSimUnits(_bodyRadius) * VIRUS_CENTER_FACTOR))
            {
                virus.Consumed();

                if (o2 == _leftHandGrabber.GrabbedVirus())
                {
                    _leftHandGrabber.CenterCollision();
                }
                if (o2 == _rightHandGrabber.GrabbedVirus())
                {
                    _rightHandGrabber.CenterCollision();
                }
            }

            return false;
        }

        public override void Draw(SpriteBatch batch)
        {
            batch.End();
            GraphicsDevice graphicsDevice = batch.GraphicsDevice;
            VertexPositionNormalTexture[] vertices = getSoftBodyVertices();
            //VertexPositionColor[] verts = getSoftBodyBlueVertices();

            _bodyEffect.View = PlayWindow.Camera2D.View;
            _bodyEffect.Projection = PlayWindow.Camera2D.DisplayProjection;
            _bodyEffect.Texture = _cellTexture;

            foreach (var pass in _bodyEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
            }

            batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, PlayWindow.Camera2D.View);

            foreach (Sprite sprite in _spriteDict.Values)
                GraphicsHandler.DrawSprite(sprite, batch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            Vector2 force = Strategy.GetAcceleration() * 0.3f;
            foreach (var item in _outerBodies)
            {
                item.ApplyForce(ref force);
            }

            PositionHelper(Position);

            HandleVirusGrabbing();
            HandleBlinkState(gameTime);
        }
    }
}

