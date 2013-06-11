using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen
{
    class Sprite
    {
        #region Field (spriteBatch, texture, position, source, color, rotation, origin, scale, effects, layer, size, textureName, frames, isShowing, isAnimated, isLooped, isStarted, curremtFrame, framesPerSecond, totalFrames)

        private Texture2D texture;
        private Vector2 position;
        private Rectangle source;
        private Color color;
        private float rotation;
        private Vector2 origin;
        private Vector2 scale;
        private float layer;
        private Vector2 size;
        private bool isVisible;
        private SpriteEffects effects;

        /* Animation properties */
        private List<Rectangle> frames;
        private bool isAnimated;
        private bool isLooped;
        private bool isStarted;
        private float currentFrame;
        private float framesPerSecond;
        private int totalFrames;
        private bool isBackwards;
        #endregion

        #region Properties(Position, Size, Rotation, Layer, Texture, IsShowing, IsStarted, NumFrames, FramesPerSecond, CurrentFrame, IsAnimated, IsLooped, IsBackwards)
       
        public Vector2 Position
        {
            get { return position; }
            set
            {
                    position = value;
            }
        }

        private Vector2 scaledSize;

        public Vector2 ScaledSize
        {
            get { return scaledSize; }
            set { scaledSize = value; }
        }
        

        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                ScaledSize = size * scale;
            }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set 
            { 
                texture = value;
                Size = new Vector2(texture.Width, texture.Height);
            }
        }

        public Color Color 
        { 
            get { return color; }
            set { color = value; }
        }

        public Rectangle Source
        {
            get { return source; }
            set { source = value; }
        }
      
        public Vector2 Origin
        {
            get { return origin; }
            set { origin = value; }
        }
        
        public Vector2 Scale
        {
            get { return scale; }
            set 
            { 
                scale = value;
                ScaledSize = size * scale;
            }
        }

        public SpriteEffects Effects
        {
            get { return effects; }
            set { effects = value; }
        }

        /// <summary>
        /// Returns the rotation of the sprite in radians
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Returns the layer as a float, (The sprite with the lowest layer value will drawn on top. Must be between 0 and 1)
        /// </summary>
        public float Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        public bool IsShowing
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        #region getters for Animation properties

        /// <summary>
        /// Returns a bool that indicates wether the animation is started or not
        /// </summary>
        public bool IsStarted
        {
            get { return isStarted; }
            set { isStarted = value; }
        }

        /// <summary>
        /// Gets or sets the number of frames in the animation
        /// </summary>
        public int NumFrames
        {
            get { return totalFrames; }
            set { totalFrames = value; }
        }

        /// <summary>
        /// Gets or sets the fps for the animation
        /// </summary>
        public float FramesPerSecond
        {
            get { return framesPerSecond; }
            set
            {
                if (framesPerSecond != value && value > 0)
                    framesPerSecond = value / 1000;
            }
        }

        /// <summary>
        /// Gets or sets the current frame showing
        /// </summary>
        public float CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                if (currentFrame != value && value >= 0 && value <= totalFrames)
                    this.currentFrame = value;
            }
        }

        public bool IsAnimated
        {
            get { return isAnimated; }
            set { isAnimated = value; }
        }

        public bool IsLooped
        {
            get { return this.isLooped; }
            set { isLooped = value; }
        }

        public bool IsBackwards
        {
            get { return isBackwards; }
            set { isBackwards = value; }
        }
        #endregion
        #endregion

        public virtual void Initialize()
        {
            this.position = Vector2.Zero;
            this.isVisible = true;
            this.color = Color.White;
            this.rotation = 0f;
            this.scale = Vector2.One;
            this.effects = SpriteEffects.None;
            this.layer = 0.5f;
            this.size = Vector2.Zero;
            this.isAnimated = false;
            this.totalFrames = 0;
            this.framesPerSecond = 0f;
            this.isBackwards = false;
        }
    }
}
