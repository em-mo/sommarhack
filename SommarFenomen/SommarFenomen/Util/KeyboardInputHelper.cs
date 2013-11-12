using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace SommarFenomen.Util
{
    class KeyboardInputHelper
    {
        KeyboardState previousState, currentState;

        public KeyboardInputHelper ()
	    {
            previousState = currentState = Keyboard.GetState();
    	}

        public bool isKeyPressed(Keys key)
        {
            return currentState.IsKeyDown(key) && previousState.IsKeyUp(key);
        }

        public bool isKeyReleased(Keys key)
        {
            return currentState.IsKeyUp(key) && previousState.IsKeyDown(key);
        }

        public bool isKeyDown(Keys key)
        {
            return currentState.IsKeyDown(key);
        }

        public bool isKeyUp(Keys key)
        {
            return currentState.IsKeyUp(key);
        }

        public Keys[] GetKeyDowns()
        {
            return _keyDowns;
        }

        Keys[] _keyDowns;

        public void update()
        {
            previousState = currentState;
            currentState = Keyboard.GetState();

            Keys[] previousKeys = previousState.GetPressedKeys();
            Keys[] currentKeys = currentState.GetPressedKeys();

            _keyDowns = currentKeys.Except(previousKeys).ToArray();
        }
    }
}
