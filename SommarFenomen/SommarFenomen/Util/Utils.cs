using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen
{
    enum Arm { Left, Right };
    enum Direction { None, Left, Right, Up, Down};

    // Angle in radians
    class WindPuffMessage
    {
        private float direction;
        private Vector2 position;
        private DateTime ttl;

        public WindPuffMessage(float direction, Vector2 position)
        {
            this.direction = direction;
            this.position = position;
            ttl = DateTime.Now.AddSeconds(1);
        }

        public Vector2 Position
        {
            get { return position; }
            set { this.position = value; }
        }

        public float Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public bool checkAge()
        {
            if (ttl < DateTime.Now)
                return true;
            else
                return false;
        }
    }

    class Utils
    {
        public static double CalculateAngle(Vector3 vector1, Vector3 vector2)
        {
            vector1.Normalize();
            vector2.Normalize();
            
            Vector3 crossProduct = Vector3.Cross(vector1, vector2);

            double crossProductLength = crossProduct.Z;

            double dotProduct = Vector3.Dot(vector1, vector2);

            double angle = Math.Atan2(crossProductLength, dotProduct);

            return angle;
        }

        public static double CalculateAngle(Vector2 vector1, Vector2 vector2)
        {
            return Math.Atan2(vector2.Y, vector2.X) - Math.Atan2(vector1.Y, vector1.X);
        }

        public static void AddToSpritePosition(Sprite sprite, Vector2 vector)
        {
            Vector2 newPosition = sprite.Position + vector;           
            sprite.Position = newPosition;
        }

        public static void AddToSpritePosition(Sprite sprite, float x, float y)
        {
            Vector2 newPosition = sprite.Position;
            newPosition.X += x;
            newPosition.Y += y;
            sprite.Position = newPosition;
        }

        public static double TicksToSeconds(long ticks)
        {
            return (double)ticks / (double)10000000;
        }

        public static Vector2 AddToVector(Vector2 vector, float x)
        {
            vector.X += x;
            vector.Y += x;

            return vector;
        }

        public static Texture2D MergeTextures(Texture2D bottomTexture, Texture2D topTexture, GraphicsDevice device)
        {
            return MergeTextures(bottomTexture, topTexture, Vector2.Zero, Vector2.Zero, device);
        }

        public static Texture2D MergeTextures(Texture2D bottomTexture, Texture2D topTexture, Vector2 offset, GraphicsDevice device)
        {
            Vector2 origin = new Vector2(topTexture.Width / 2, topTexture.Height / 2);
            return MergeTextures(bottomTexture, topTexture, offset, origin, device);
        }

        public static Texture2D MergeTextures(Texture2D bottomTexture, Texture2D topTexture, Vector2 offset, Vector2 origin, GraphicsDevice device)
        {
            RenderTarget2D renderTarget = new RenderTarget2D(device, bottomTexture.Bounds.Width, bottomTexture.Bounds.Height);
            device.SetRenderTarget(renderTarget);
            device.Clear(Color.Transparent);

            SpriteBatch batch = new SpriteBatch(device);
            batch.Begin();
            batch.Draw(bottomTexture, Vector2.Zero, Color.White);

            Console.WriteLine("Top offset " + offset);
            Console.WriteLine("Top origin " + origin);
            batch.Draw(topTexture, offset - origin, Color.White);

            batch.End();

            device.SetRenderTarget(null);
            return (Texture2D)renderTarget;
        }
    }
}
