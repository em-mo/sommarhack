using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace SommarFenomen.Windows
{
    class Background
    {
        BasicEffect _basicEffect;
        PlayWindow _playWindow;
        VertexPositionTexture[] vertices;

        float _scaleFactor;

        public Background(string file, PlayWindow playWindow)
        {
            _scaleFactor = 500;
            _playWindow = playWindow;

            _basicEffect = new BasicEffect(_playWindow.GraphicsDevice);
            _basicEffect.TextureEnabled = true;
            _basicEffect.Texture = Game1.contentManager.Load<Texture2D>(file);

            _scaleFactor = 1000;


            vertices = new VertexPositionTexture[4];
            vertices[0].Position.X = -10000;
            vertices[0].Position.Y = -10000;
            vertices[0].Position.Z = 0;
            vertices[0].TextureCoordinate.X = vertices[0].Position.X / _scaleFactor;
            vertices[0].TextureCoordinate.Y = vertices[0].Position.Y / _scaleFactor;

            vertices[1].Position.X = 10000;
            vertices[1].Position.Y = -10000;
            vertices[1].Position.Z = 0;
            vertices[1].TextureCoordinate.X = vertices[1].Position.X / _scaleFactor;
            vertices[1].TextureCoordinate.Y = vertices[1].Position.Y / _scaleFactor;

            vertices[2].Position.X = -10000;
            vertices[2].Position.Y = 10000;
            vertices[2].Position.Z = 0;
            vertices[2].TextureCoordinate.X = vertices[2].Position.X / _scaleFactor;
            vertices[2].TextureCoordinate.Y = vertices[2].Position.Y / _scaleFactor;

            vertices[3].Position.X = 10000;
            vertices[3].Position.Y = 10000;
            vertices[3].Position.Z = 0;
            vertices[3].TextureCoordinate.X = vertices[3].Position.X / _scaleFactor;
            vertices[3].TextureCoordinate.Y = vertices[3].Position.Y / _scaleFactor;
        }

        public void Draw(SpriteBatch batch)
        {
            GraphicsDevice graphicsDevice = batch.GraphicsDevice;


            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            //graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _basicEffect.View = _playWindow.Camera2D.View;
            _basicEffect.Projection = _playWindow.Camera2D.DisplayProjection;

            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
            }
        }
    }
}
