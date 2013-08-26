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
using FarseerPhysics.SamplesFramework;

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

            _thickness = 50.0f;

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
            if (vertices.Count < 3)
            {
                Console.WriteLine("Too few vertices " + vertices.Count);
                return;
            }

            Vertices drawingVertices = getDrawingVertices(vertices, type);

            Vertices innerVertices, outerVertices;


            if (type == WallType.Outer)
            {
                innerVertices = vertices;
                outerVertices = drawingVertices;
            }
            else
            {
                innerVertices = drawingVertices;
                outerVertices = vertices;
            }

            _wallVerts = new VertexPositionTexture[vertices.Count * 2 + 2];
            for (int i = 0; i < vertices.Count; ++i)
            {
                int j = i * 2;
                _wallVerts[j].Position.X = innerVertices[i].X;
                _wallVerts[j].Position.Y = innerVertices[i].Y;
                _wallVerts[j].Position.Z = 0;
                _wallVerts[j].TextureCoordinate = innerVertices[i] / _textureScaling;

                _wallVerts[j + 1].Position.X = outerVertices[i].X;
                _wallVerts[j + 1].Position.Y = outerVertices[i].Y;
                _wallVerts[j + 1].Position.Z = 0;
                _wallVerts[j + 1].TextureCoordinate = outerVertices[i] / _textureScaling;

            }
            int lastIndex = vertices.Count * 2;
            _wallVerts[lastIndex].Position.X = innerVertices[0].X;
            _wallVerts[lastIndex].Position.Y = innerVertices[0].Y;
            _wallVerts[lastIndex].Position.Z = 0;
            _wallVerts[lastIndex].TextureCoordinate = innerVertices[0] / _textureScaling;


            _wallVerts[lastIndex + 1].Position.X = outerVertices[0].X;
            _wallVerts[lastIndex + 1].Position.Y = outerVertices[0].Y;
            _wallVerts[lastIndex + 1].Position.Z = 0;
            _wallVerts[lastIndex + 1].TextureCoordinate = outerVertices[0] / _textureScaling;


        }

        /// <summary>
        /// Get the other set of vertices to get a thickness of the wall
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Vertices getDrawingVertices(Vertices vertices, WallType type)
        {
            Vector2 previousVertex, currentVertex, nextVertex;
            Vertices drawingVertices = new Vertices();



            previousVertex = vertices.Last();
            currentVertex = vertices.First();
            nextVertex = vertices.NextVertex(0);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 normalisedNext = (nextVertex - currentVertex);
                normalisedNext.Normalize();
                Vector2 normalisedPrevious = (previousVertex - currentVertex);
                normalisedPrevious.Normalize();

                Vector2 drawVertexDifference = normalisedNext + normalisedPrevious;
                //drawVertexDifference.Normalize();

                double angle = Utils.CalculateAngle(normalisedPrevious, normalisedNext);

                if (angle < 0)
                    angle = angle + Math.PI * 2;

                // Concave or convex corner
                if (angle > Math.PI && type == WallType.Outer)
                    drawVertexDifference = drawVertexDifference * -1;
                else if (angle < Math.PI && type == WallType.Inner)
                    drawVertexDifference = drawVertexDifference * -1;

                drawVertexDifference *= _thickness;
                Vector2 drawVertex = vertices[i] + drawVertexDifference;
                drawingVertices.Add(drawVertex);

                previousVertex = currentVertex;
                currentVertex = nextVertex;
                nextVertex = vertices[(i + 2) % vertices.Count];
            }

            return drawingVertices;
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
    }
}
