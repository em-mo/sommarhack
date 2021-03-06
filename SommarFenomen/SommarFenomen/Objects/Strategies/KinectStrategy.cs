﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SommarFenomen
{
    class KinectStrategy : Strategy
    {
        private const float KEY_SPEED = 15f;
        public Vector2 CurrentAcceleration { get; set; }

        KeyboardState oldState;

        public override Vector2 GetAcceleration() 
        {
            KeyboardState newState = Keyboard.GetState();
            Vector2 accel = CurrentAcceleration;

            if (newState.IsKeyDown(Keys.Left))
            {
                accel.X = -KEY_SPEED;
            }
            if (newState.IsKeyDown(Keys.Right))
            {
                accel.X = KEY_SPEED;
            }
            if (newState.IsKeyDown(Keys.Up))
            {
                accel.Y = -KEY_SPEED;
            }
            if (newState.IsKeyDown(Keys.Down))
            {
                accel.Y = KEY_SPEED;
            }
            
            oldState = newState;
            return accel;
        }
    }
}
