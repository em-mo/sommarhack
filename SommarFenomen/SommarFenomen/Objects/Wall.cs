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

namespace SommarFenomen.Objects
{
    class Wall : IGameObject
    {
        private Body _body;
        private BasicEffect _basicEffect;
        
        private VertexPositionColorTexture[] _wallVerts;
        
        /// <summary>
        /// Temporary
        /// </summary>
        private VertexPositionColor[] _colorWallVerts;

        private PlayWindow _playWindow;

        enum WallType
        {
            Inner, Outer
        }
        public Wall(Vertices vertices, Vector2 outDirection, PlayWindow playWindow)
        {
            _playWindow = playWindow;

            SimplifyTools.MergeParallelEdges(vertices, 0.001f);

            _body = BodyFactory.CreateLoopShape(_playWindow.World, vertices);
            _body.CollisionCategories = Category.All;
            _body.CollidesWith = Category.All;

            _basicEffect = new BasicEffect(_playWindow.GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            //_basicEffect.TextureEnabled = true;
            //_basicEffect.Texture = Game1.contentManager.Load<Texture2D>("Wall");

        }

        private void InitDrawVertices(Vertices vertices, WallType type)
        {

            if (vertices.Count < 3)
            {
                Console.WriteLine("Too few vertices " + vertices.Count);
                return;
            }

            Vertices outerVertices = getOuterVertices(vertices);

            if (outerVertices.GetArea() < vertices.GetArea() && type == WallType.Outer)
                outerVertices = getOuterVertices(vertices);

            _wallVerts = new VertexPositionColorTexture[1];
            //for (int i = 0; i < vertices.Count; i++)
            //{
            //    _wallVerts.
            //}
            
        }

        private Vertices getOuterVertices(Vertices vertices)
        {
            Vector2 previousVertex, currentVertex, nextVertex;

            previousVertex = vertices.Last();
            currentVertex = vertices.First();
            nextVertex = vertices.NextVertex(0);

            for (int i = 0; i < vertices.Count; i++)
            {
                double angle = Utils.CalculateAngle(previousVertex, nextVertex);
            }

            return new Vertices();
        }
    }
}
