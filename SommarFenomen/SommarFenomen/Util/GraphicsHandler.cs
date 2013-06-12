using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SommarFenomen
{
    class GraphicsHandler
    {
        public static void DrawSprites(List<Sprite> spriteList, SpriteBatch batch)
        {
            foreach (Sprite sprite in spriteList)
	        {
                if(sprite.OriginalSize != Vector2.Zero && sprite.IsShowing)
                    batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
                else if (sprite.IsShowing)
                    batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
            }       
        }

        public static void DrawSprite(Sprite sprite, SpriteBatch batch)
        {
            if (sprite.OriginalSize != Vector2.Zero && sprite.IsShowing)
                batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
            else if (sprite.IsShowing)
                batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
        }
    }
}
