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
        private VertexPositionColorTexture[] _wallVerts;
        
        /// <summary>
        /// Temporary
        /// </summary>
        private VertexPositionColor[] _colorWallVerts;

        private PlayWindow _playWindow;

        public enum WallType
        {
            Inner, Outer
        }
        public Wall(Vertices vertices, WallType type, PlayWindow playWindow)
        {
            _playWindow = playWindow;

            _thickness = 10.0f;
            
            SimplifyTools.MergeParallelEdges(vertices, 0.001f);

            _body = BodyFactory.CreateLoopShape(_playWindow.World, VerticesToSimUnits(vertices));
            _body.CollisionCategories = Category.All;
            _body.CollidesWith = Category.All;

            _basicEffect = new BasicEffect(_playWindow.GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            //_basicEffect.TextureEnabled = true;
            //_basicEffect.Texture = Game1.contentManager.Load<Texture2D>("Wall");

            InitDrawVertices(vertices, type);
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

            _colorWallVerts = new VertexPositionColor[vertices.Count * 2 + 2];
            for (int i = 0; i < vertices.Count; i = i + 2)
            {
                _colorWallVerts[i].Position.X = outerVertices[i].X;
                _colorWallVerts[i].Position.Y = outerVertices[i].Y;
                _colorWallVerts[i].Position.Z = 0;
                _colorWallVerts[i].Color = Color.Black;

                _colorWallVerts[i + 1].Position.X = innerVertices[i].X;
                _colorWallVerts[i + 1].Position.Y = innerVertices[i].Y;
                _colorWallVerts[i + 1].Position.Z = 0;
                _colorWallVerts[i + 1].Color = Color.Black;
            }

            _colorWallVerts[vertices.Count].Position.X = outerVertices[0].X;
            _colorWallVerts[vertices.Count].Position.Y = outerVertices[0].Y;
            _colorWallVerts[vertices.Count].Position.Z = 0;
            _colorWallVerts[vertices.Count].Color = Color.Black;

            _colorWallVerts[vertices.Count + 1].Position.X = innerVertices[0].X;
            _colorWallVerts[vertices.Count + 1].Position.Y = innerVertices[0].Y;
            _colorWallVerts[vertices.Count + 1].Position.Z = 0;
            _colorWallVerts[vertices.Count + 1].Color = Color.Black; 
            
        }

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
                drawVertexDifference.Normalize();

                double angle = Utils.CalculateAngle(normalisedPrevious, normalisedNext);

                if (angle < 0)
                    angle = angle + Math.PI * 2;

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

            _basicEffect.View = _playWindow.Camera2D.View;
            _basicEffect.Projection = _playWindow.Camera2D.DisplayProjection;

            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, _colorWallVerts, 0, _colorWallVerts.Length / 3);
                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, _colorWallVerts, 0, _colorWallVerts.Length / 2);
            }
        }
    }
}
