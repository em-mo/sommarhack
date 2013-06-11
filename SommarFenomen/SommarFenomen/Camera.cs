using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen
{
    public class Camera
    {
        public Camera(Viewport viewport)
        {
            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
            Zoom = 1.0f;
            Position = Vector2.Zero;
            this.viewport = viewport;

            followBoundaries.X = viewport.Width / 4f;
            followBoundaries.Y = viewport.Height / 4f;

            limits = null;
        }
        private Viewport viewport;

        private Vector2 followBoundaries;

        public Vector2 FollowBoundaries
        {
            get { return followBoundaries; }
            set { followBoundaries = value; }
        }
        
        public Vector2 Origin { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }

        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;

                // If there's a limit set and the camera is not transformed clamp position to limits
                if (Limits != null && Zoom == 1.0f && Rotation == 0.0f)
                {
                    position.X = MathHelper.Clamp(position.X, Limits.Value.X, Limits.Value.X + Limits.Value.Width - viewport.Width);
                    position.Y = MathHelper.Clamp(position.Y, Limits.Value.Y, Limits.Value.Y + Limits.Value.Height - viewport.Height);
                }
            }
        }
        
        private Rectangle? limits;
        public Rectangle? Limits
        {
            get { return limits; }
            set
            {
                if (value != null)
                {
                    // Assign limit but make sure it's always bigger than the viewport
                    limits = new Rectangle
                    {
                        X = value.Value.X,
                        Y = value.Value.Y,
                        Width = Math.Max(viewport.Width, value.Value.Width),
                        Height = Math.Max(viewport.Height, value.Value.Height)
                    };

                    // Validate camera position with new limit
                    Position = Position;
                }
                else
                {
                    limits = null;
                }
            }
        }

        public void LookAt(Vector2 target)
        {
            Position = target - new Vector2(viewport.Width / 2f, viewport.Height / 2f);
        }

        public void Follow(Vector2 target)
        {
            Vector2 newPosition = Position;
            if (target.X < position.X + FollowBoundaries.X)
                newPosition.X = target.X - FollowBoundaries.X;
            else if (target.X > position.X + viewport.Width - FollowBoundaries.X)
                newPosition.X = target.X - viewport.Width + FollowBoundaries.X;

            if (target.Y < position.Y + FollowBoundaries.Y)
                newPosition.Y = target.Y - FollowBoundaries.Y;
            else if (target.Y > position.Y + viewport.Height - FollowBoundaries.Y)
                newPosition.Y = target.Y - viewport.Height + FollowBoundaries.Y;

            Position = newPosition;
        }

        public Matrix GetViewMatrix(Vector2 parallax)
        {
            // To add parallax, simply multiply it by the position
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                // The next line has a catch. See note below.
                   Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateScale(Zoom, Zoom, 1) *
                   Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }
    }
}
