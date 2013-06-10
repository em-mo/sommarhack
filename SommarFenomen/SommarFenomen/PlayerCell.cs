using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SommarFenomen
{
    class PlayerCell : ActiveGameObject
    {
        enum PlayerSprites { Cloud, LeftHumerus, LeftUlna, LeftHand, RightHumerus, RightUlna, RightHand };

        private Dictionary<Direction, Texture2D> cloudTextures;
        private Sprite windPuff;

        private const float DRAG_ACCELERATION = -150;
        private const float MAX_SPEED = 500;
        private const float ARM_SCALE = 0.75f;
        private static float DIRECTION_SPRITE_THRESHOLD = 100;

        private float rightHumerusOffsetX;
        private float rightHumerusOffsetY;
        private float rightUlnaOffset;
        private float rightHandOffset;
        private float leftHumerusOffsetX;
        private float leftHumerusOffsetY;
        private float leftUlnaOffset;
        private float leftHandOffset;

        private List<WindPuffMessage> windPuffList = new List<WindPuffMessage>();

        private Dictionary<PlayerSprites, Sprite> spriteDict;

        private Vector2 position;

        public readonly object locker = new object();

        //Sets position of all sprites
        private void PositionHelper(Vector2 v)
        {
            Vector2 diffVector = v - position;

            foreach (Sprite sprite in spriteDict.Values)
                Utils.AddToSpritePosition(sprite, diffVector);

            spriteDict[PlayerSprites.Cloud].Position = v;
            position = v;
        }

        public PlayerCell(Vector2 screenOffset)
        {
            spriteDict = new Dictionary<PlayerSprites, Sprite>();
            InitSprites();
            Position = new Vector2(600, 100);
            InitArms();

            SetLeftArmRotation((float)Math.PI / 2, (float)Math.PI / 2);
            SetRightArmRotation(-(float)Math.PI / 2, -(float)Math.PI / 2);
        }

        private void InitSprites()
        {

            spriteDict = new Dictionary<PlayerSprites, Sprite>();

            foreach (PlayerSprites sprite in Enum.GetValues(typeof(PlayerSprites)))
            {
                spriteDict.Add(sprite, new Sprite());
                spriteDict[sprite].Initialize();
            }
            windPuff = new Sprite();
            windPuff.Initialize();

            cloudTextures = new Dictionary<Direction, Texture2D>();
            cloudTextures.Add(Direction.None, Game1.contentManager.Load<Texture2D>(@"Images\Cloud"));
            cloudTextures.Add(Direction.Left, Game1.contentManager.Load<Texture2D>(@"Images\Cloud_Move_Left"));
            cloudTextures.Add(Direction.Right, Game1.contentManager.Load<Texture2D>(@"Images\Cloud_Move_Right"));

            windPuff.Texture = Game1.contentManager.Load<Texture2D>(@"Images\wind");
            spriteDict[PlayerSprites.Cloud].Texture = cloudTextures[Direction.None];
            spriteDict[PlayerSprites.LeftHumerus].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Humerus_left");
            spriteDict[PlayerSprites.LeftHand].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Hand_left");
            spriteDict[PlayerSprites.RightHumerus].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Humerus_right");
            spriteDict[PlayerSprites.RightHand].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Hand_right");
            spriteDict[PlayerSprites.LeftUlna].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Ulna_left");
            spriteDict[PlayerSprites.RightUlna].Texture = Game1.contentManager.Load<Texture2D>(@"Images\Ulna_right");

            spriteDict[PlayerSprites.Cloud].Scale = Vector2.One * 0.6f;

            // Origin to right mid
            spriteDict[PlayerSprites.LeftHumerus].Origin = new Vector2(spriteDict[PlayerSprites.LeftHumerus].Size.X, spriteDict[PlayerSprites.LeftHumerus].Size.Y / 2);
            spriteDict[PlayerSprites.LeftUlna].Origin = new Vector2(spriteDict[PlayerSprites.LeftUlna].Size.X, spriteDict[PlayerSprites.LeftUlna].Size.Y / 2);
            spriteDict[PlayerSprites.LeftHand].Origin = new Vector2(spriteDict[PlayerSprites.LeftHand].Size.X, spriteDict[PlayerSprites.LeftHand].Size.Y * 5 / 7);
            /*
            //Scale Left
            spriteDict[PlayerSprites.LeftHumerus].Scale = new Vector2(armScale);
            spriteDict[PlayerSprites.LeftUlna].Scale = new Vector2(armScale);
            spriteDict[PlayerSprites.LeftHand].Scale = new Vector2(armScale);
            */

            //Origin to left mid
            spriteDict[PlayerSprites.RightHumerus].Origin = new Vector2(0, spriteDict[PlayerSprites.RightHumerus].Size.Y / 2);
            spriteDict[PlayerSprites.RightUlna].Origin = new Vector2(0, spriteDict[PlayerSprites.RightUlna].Size.Y / 2);
            spriteDict[PlayerSprites.RightHand].Origin = new Vector2(0, spriteDict[PlayerSprites.RightHand].Size.Y * 5 / 7);
            /*
            //Scale Right
            spriteDict[PlayerSprites.RightHumerus].Scale = new Vector2(armScale);
            spriteDict[PlayerSprites.RightUlna].Scale = new Vector2(armScale);
            spriteDict[PlayerSprites.RightHand].Scale = new Vector2(armScale);
            */
            //Origin center
            windPuff.Origin = new Vector2(windPuff.Size.X / 2, windPuff.Size.Y / 2);
        }

        public void Update(GameTime gameTime)
        {
            base.update(gameTime);
        }

        private void InitArms()
        {
            leftHumerusOffsetX = (float)(spriteDict[PlayerSprites.Cloud].Size.X * 0.1);
            leftHumerusOffsetY = (float)(spriteDict[PlayerSprites.Cloud].Size.Y * 0.6);
            leftUlnaOffset = -(float)(spriteDict[PlayerSprites.LeftHumerus].Size.X * 0.95);
            leftHandOffset = -(float)(spriteDict[PlayerSprites.LeftUlna].Size.X * 0.97);

            rightHumerusOffsetX = (float)(spriteDict[PlayerSprites.Cloud].Size.X * 0.8);
            rightHumerusOffsetY = (float)(spriteDict[PlayerSprites.Cloud].Size.Y * 0.6);
            rightUlnaOffset = (float)(spriteDict[PlayerSprites.RightHumerus].Size.X * 0.95);
            rightHandOffset = (float)(spriteDict[PlayerSprites.RightUlna].Size.X * 0.97);

            //Set left
            Vector2 newHumerusPosition = new Vector2();
            newHumerusPosition.X = position.X + leftHumerusOffsetX;
            newHumerusPosition.Y = position.Y + leftHumerusOffsetY;

            Vector2 newUlnaPosition = new Vector2();
            newUlnaPosition.X = newHumerusPosition.X + leftUlnaOffset;
            newUlnaPosition.Y = newHumerusPosition.Y;

            Vector2 newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + leftHandOffset;
            newHandPosition.Y = newUlnaPosition.Y;

            spriteDict[PlayerSprites.LeftHumerus].Position = newHumerusPosition;
            spriteDict[PlayerSprites.LeftUlna].Position = newUlnaPosition;
            spriteDict[PlayerSprites.LeftHand].Position = newHandPosition;

            //Set right
            newHumerusPosition = new Vector2();
            newHumerusPosition.X = position.X + rightHumerusOffsetX;
            newHumerusPosition.Y = position.Y + rightHumerusOffsetY;

            newUlnaPosition = new Vector2();
            newUlnaPosition.X = newHumerusPosition.X + rightUlnaOffset;
            newUlnaPosition.Y = newHumerusPosition.Y;

            newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + rightHandOffset;
            newHandPosition.Y = newUlnaPosition.Y;

            spriteDict[PlayerSprites.RightHumerus].Position = newHumerusPosition;
            spriteDict[PlayerSprites.RightUlna].Position = newUlnaPosition;
            spriteDict[PlayerSprites.RightHand].Position = newHandPosition;

        }

        /// <summary>
        /// Rotates all three sections of the arm
        /// </summary>
        /// <param name="humerusRotation">Clockwise rotation in radians</param>
        /// <param name="ulnaRotation">Clockwise rotation in radians</param>
        public void SetLeftArmRotation(float humerusRotation, float ulnaRotation)
        {

            spriteDict[PlayerSprites.LeftHumerus].Rotation = humerusRotation;

            Vector2 newUlnaPosition = new Vector2();
            newUlnaPosition.X = spriteDict[PlayerSprites.LeftHumerus].Position.X + (float)(Math.Cos(humerusRotation) * leftUlnaOffset);
            newUlnaPosition.Y = spriteDict[PlayerSprites.LeftHumerus].Position.Y + (float)(Math.Sin(humerusRotation) * leftUlnaOffset);

            Vector2 newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + (float)(Math.Cos(ulnaRotation) * leftHandOffset);
            newHandPosition.Y = newUlnaPosition.Y + (float)(Math.Sin(ulnaRotation) * leftHandOffset);

            lock (locker)
            {
                spriteDict[PlayerSprites.LeftUlna].Position = newUlnaPosition;
                spriteDict[PlayerSprites.LeftUlna].Rotation = ulnaRotation;

                spriteDict[PlayerSprites.LeftHand].Position = newHandPosition;
                spriteDict[PlayerSprites.LeftHand].Rotation = ulnaRotation;
            }
        }

        /// <summary>
        /// Rotates all three sections of the arm
        /// </summary>
        /// <param name="humerusRotation">Clockwise rotation in radians</param>
        /// <param name="ulnaRotation">Clockwise rotation in radians</param>
        public void SetRightArmRotation(float humerusRotation, float ulnaRotation)
        {

            spriteDict[PlayerSprites.RightHumerus].Rotation = humerusRotation;

            Vector2 newUlnaPosition = new Vector2();
            newUlnaPosition.X = spriteDict[PlayerSprites.RightHumerus].Position.X + (float)(Math.Cos(humerusRotation) * rightUlnaOffset);
            newUlnaPosition.Y = spriteDict[PlayerSprites.RightHumerus].Position.Y + (float)(Math.Sin(humerusRotation) * rightUlnaOffset);

            Vector2 newHandPosition = new Vector2();
            newHandPosition.X = newUlnaPosition.X + (float)(Math.Cos(ulnaRotation) * rightHandOffset);
            newHandPosition.Y = newUlnaPosition.Y + (float)(Math.Sin(ulnaRotation) * rightHandOffset);

            lock (locker)
            {
                spriteDict[PlayerSprites.RightUlna].Position = newUlnaPosition;
                spriteDict[PlayerSprites.RightUlna].Rotation = ulnaRotation;

                spriteDict[PlayerSprites.RightHand].Position = newHandPosition;
                spriteDict[PlayerSprites.RightHand].Rotation = ulnaRotation;
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
                hand = spriteDict[PlayerSprites.LeftHand];
                offset = -hand.Size.X;
            }
            else
            {
                hand = spriteDict[PlayerSprites.RightHand];
                offset = hand.Size.X;
            }

            Vector2 position = new Vector2(hand.Position.X + offset, hand.Position.Y);

            windPuffList.Add(new WindPuffMessage(rotation, position));
        }

        

        private void DrawWindPuff(GraphicsHandler g)
        {
            WindPuffMessage puff;
            for (int i = windPuffList.Count - 1; i >= 0; i--)
            {
                puff = windPuffList.ElementAt(i);

                windPuff.Position = puff.Position;
                windPuff.Rotation = puff.Direction;

                g.DrawSprite(windPuff);

                if (puff.checkAge())
                    windPuffList.RemoveAt(i);
            }
        }

        public void DrawSick(GraphicsHandler g)
        {
            Color color = Color.ForestGreen;
            color.A = 128;
            spriteDict[PlayerSprites.Cloud].Color = color;
            g.DrawSprite(spriteDict[PlayerSprites.Cloud]);
            spriteDict[PlayerSprites.Cloud].Color = Color.White;
        }

        public void Draw(GraphicsHandler g)
        {
            lock (locker)
            {
                DrawWindPuff(g);
                foreach (Sprite sprite in spriteDict.Values)
                    g.DrawSprite(sprite);
            }
        }
    }
}

