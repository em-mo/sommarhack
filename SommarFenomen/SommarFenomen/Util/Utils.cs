using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

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
    }
}
