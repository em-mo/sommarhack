using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Kinect;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using SommarFenomen.Objects;

namespace SommarFenomen
{
    class PlayWindow : Window
    {
        Sprite background;
        SpriteBatch spriteBatch;
        GraphicsDevice graphicsDevice;
        KeyboardState oldState;
        Camera camera;

        List<Sprite> spriteList = new List<Sprite>();
        List<Sprite> backgroundSprites = new List<Sprite>();

        Random rand = new Random();

        // Chooses algorithm for hand movement
        private bool movementType = true;

        private PlayerCell player;

        public PlayerCell Player
        {
            get { return player; }
        }

        public bool MovmentType
        {
            get { return movementType; }
        }

        private void ToggleMovementType()
        {
            if (movementType)
                movementType = false;
            else
                movementType = true;

            System.Console.WriteLine(movementType);
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
            oldState = new KeyboardState();
            camera = new Camera(graphicsDevice.Viewport);

            background = new Sprite(Game1.contentManager.Load<Texture2D>(@"Images\Gradient"));

            player = new PlayerCell();
            GoodCell.LoadContent();
            backgroundSprites.Add(background);
        }

        private void KeyboardInput()
        {
            #region Key States
            KeyboardState newState = Keyboard.GetState();

            //KEY SPACE
            if (newState.IsKeyDown(Keys.Space) && !oldState.IsKeyDown(Keys.Space))
            {
                ToggleMovementType();
            }

            oldState = newState;
            #endregion
        }

        private void ApplyGlobalForces(GameTime gameTime)
        {
            const float DRAG_COEFFICIENT = -1f;
            player.AddAcceleration(player.Speed * DRAG_COEFFICIENT, gameTime);
        }

        private void MoveCamera()
        {
            camera.Follow(player.getCenter());
        }

        public void Update(GameTime gameTime)
        {
            ApplyGlobalForces(gameTime);

            player.Update(gameTime);
            CheckForCloudCollision();

            MoveCamera();
            KeyboardInput();
        }

        /// <summary>
        /// Checks for collisions between player and poison clouds
        /// </summary>
        private void CheckForCloudCollision()
        {
            BoundingRect playerRect = player.Bounds;
        }


        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.GetViewMatrix(new Vector2(1, 1)));
            GraphicsHandler.DrawSprites(backgroundSprites, spriteBatch);
            GraphicsHandler.DrawSprites(spriteList, spriteBatch);
            player.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
