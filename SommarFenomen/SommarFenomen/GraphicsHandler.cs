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
        SpriteBatch batch;

        public void Initialize(SpriteBatch batch)
        {
            this.batch = batch;
        }

        public void DrawSprites(List<Sprite> spriteList)
        {
            foreach (Sprite sprite in spriteList)
	        {
                if(sprite.Size != Vector2.Zero && sprite.IsShowing)
                    batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
                else if (sprite.IsShowing)
                    batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
            }       
        }

        public void DrawSprite(Sprite sprite)
        {
            if (sprite.Size != Vector2.Zero && sprite.IsShowing)
                batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
            else if (sprite.IsShowing)
                batch.Draw(sprite.Texture, sprite.Position, null, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Effects, sprite.Layer);
        }
    }
}
