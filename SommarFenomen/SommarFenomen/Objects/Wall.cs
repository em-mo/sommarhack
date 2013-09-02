using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common.PolygonManipulation;
using SommarFenomen.Util;

namespace SommarFenomen.Objects
{
    class Wall : IGameObject
    {
        private Body _body;
        private BasicEffect _basicEffect;
        private float _thickness;
        private VertexPositionTexture[] _wallVerts;
        private int _textureScaling = 100;

        private PlayWindow _playWindow;

        public enum WallType
        {
            Inner, Outer
        }
        public Wall(Vertices vertices, WallType type, PlayWindow playWindow)
        {
            _playWindow = playWindow;

            _thickness = 100.0f;

            Vertices wallVertices = SimplifyTools.CollinearSimplify(vertices);
            _body = BodyFactory.CreateLoopShape(_playWindow.World, VerticesToSimUnits(wallVertices));
            _body.CollisionCategories = Category.All;
            _body.CollidesWith = Category.All;

            _basicEffect = new BasicEffect(_playWindow.GraphicsDevice);
            _basicEffect.TextureEnabled = true;
            _basicEffect.Texture = Game1.contentManager.Load<Texture2D>(@"Images\Walls\Wall_1");

            InitDrawVertices(wallVertices, type);
        }

        private void InitDrawVertices(Vertices vertices, WallType type)
        {
            if (type == WallType.Inner)
                InitInnerWall(vertices);
            else
                InitOuterWall(vertices);
        }

        private void InitInnerWall(Vertices vertices)
        {
            if (vertices.Count < 3)
            {
                Console.WriteLine("Too few vertices " + vertices.Count);
                return;
            }

            Vector2 center = vertices.GetCentroid();

            _wallVerts = new VertexPositionTexture[vertices.Count * 2 + 2];
            for (int i = 0; i < vertices.Count; ++i)
            {
                int j = i * 2;
                _wallVerts[j].Position.X = vertices[i].X;
                _wallVerts[j].Position.Y = vertices[i].Y;
                _wallVerts[j].Position.Z = 0;
                _wallVerts[j].TextureCoordinate = vertices[i] / _textureScaling;

                _wallVerts[j + 1].Position.X = center.X;
                _wallVerts[j + 1].Position.Y = center.Y;
                _wallVerts[j + 1].Position.Z = 0;
                _wallVerts[j + 1].TextureCoordinate = center / _textureScaling;

            }
            int lastIndex = vertices.Count * 2;
            _wallVerts[lastIndex].Position.X = vertices[0].X;
            _wallVerts[lastIndex].Position.Y = vertices[0].Y;
            _wallVerts[lastIndex].Position.Z = 0;
            _wallVerts[lastIndex].TextureCoordinate = vertices[0] / _textureScaling;


            _wallVerts[lastIndex + 1].Position.X = center.X;
            _wallVerts[lastIndex + 1].Position.Y = center.Y;
            _wallVerts[lastIndex + 1].Position.Z = 0;
            _wallVerts[lastIndex + 1].TextureCoordinate = center / _textureScaling;
        }

        private void InitOuterWall(Vertices vertices)
        {
            if (vertices.Count < 3)
            {
                Console.WriteLine("Too few vertices " + vertices.Count);
                return;
            }

            Vertices drawingVertices = new Vertices(vertices);
            float scale = _thickness / drawingVertices.GetRadius() + 1;
            Vector2 scaleVector = new Vector2(scale);
            Vector2 translation = drawingVertices.GetCentroid();
            drawingVertices.Translate(-translation);
            drawingVertices.Scale(ref scaleVector);
            drawingVertices.Translate(translation);

            _wallVerts = new VertexPositionTexture[vertices.Count * 2 + 2];
            for (int i = 0; i < vertices.Count; ++i)
            {
                int j = i * 2;
                _wallVerts[j].Position.X = vertices[i].X;
                _wallVerts[j].Position.Y = vertices[i].Y;
                _wallVerts[j].Position.Z = 0;
                _wallVerts[j].TextureCoordinate = vertices[i] / _textureScaling;

                _wallVerts[j + 1].Position.X = drawingVertices[i].X;
                _wallVerts[j + 1].Position.Y = drawingVertices[i].Y;
                _wallVerts[j + 1].Position.Z = 0;
                _wallVerts[j + 1].TextureCoordinate = drawingVertices[i] / _textureScaling;

            }
            int lastIndex = vertices.Count * 2;
            _wallVerts[lastIndex].Position.X = vertices[0].X;
            _wallVerts[lastIndex].Position.Y = vertices[0].Y;
            _wallVerts[lastIndex].Position.Z = 0;
            _wallVerts[lastIndex].TextureCoordinate = vertices[0] / _textureScaling;


            _wallVerts[lastIndex + 1].Position.X = drawingVertices[0].X;
            _wallVerts[lastIndex + 1].Position.Y = drawingVertices[0].Y;
            _wallVerts[lastIndex + 1].Position.Z = 0;
            _wallVerts[lastIndex + 1].TextureCoordinate = drawingVertices[0] / _textureScaling;
        }

        private Vertices VerticesToSimUnits(Vertices vertices)
        {
            Vertices simVertices = new Vertices();

            for (int i = 0; i < vertices.Count; i++)
            {
                simVertices.Add(ConvertUnits.ToSimUnits(vertices[i]));
            }

            return simVertices;
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch batch)
        {
            GraphicsDevice graphicsDevice = batch.GraphicsDevice;
            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            _basicEffect.View = _playWindow.Camera2D.View;
            _basicEffect.Projection = _playWindow.Camera2D.DisplayProjection;

            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                //graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, _colorWallVerts, 0, _colorWallVerts.Length - 2);
                graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, _wallVerts, 0, _wallVerts.Length - 2);
                //graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, _colorWallVerts, 0, _colorWallVerts.Length - 1);
            }
        }

        private Vertices DeepClone(Vertices original)
        {
            Vertices clone = new Vertices();

            foreach (var item in original)
            {
                var copy = item;
                clone.Add(copy);
            }
            return clone;
        }
    }
}
