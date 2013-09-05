using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects;
using FarseerPhysics.Common;
using System.Diagnostics;

namespace SommarFenomen.LevelHandling
{
    class LevelParser
    {
        #region Defines
        private static readonly Color PLAYER_COLOR = new Color(0, 255, 0);
        private static readonly Color FRIENDLY_COLOR = new Color(0, 0, 255);
        private static readonly Color ENEMY_COLOR = new Color(255, 0, 0);

        private static readonly Point[] _offsetPoints = new Point[] 
                {new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1),
                    new Point(-1, -1), new Point(-1, 1), new Point(1, -1), new Point(1, 1)};

        #endregion

        public PlayWindow PlayWindow { get; set; }
        int _imageWidth, _imageHeight;
        float _levelScaleFactor;

        public LevelParser(PlayWindow playWindow)
        {
            PlayWindow = playWindow;
        }

        public Level Parse(Texture2D mapImage)
        {
            Level level = new Level();

            _levelScaleFactor = 4;
            _imageHeight = mapImage.Height;
            _imageWidth = mapImage.Width;

            Color[] colorData;
            colorData = new Color[_imageWidth * _imageHeight];
            mapImage.GetData<Color>(colorData);

            for (int y = 0; y < _imageHeight; y++)
            {
                for (int x = 0; x < _imageWidth; x++)
                {
                    Color currentPixel = colorData[_imageWidth * y + x];
                    if (currentPixel == Color.White)
                        continue;

                    Point position = new Point(x, y);

                    //Wall if all are the same
                    if (currentPixel.R == currentPixel.G && currentPixel.G == currentPixel.B)
                        HandleWall(position, currentPixel, colorData, level);
                    else if (currentPixel == PLAYER_COLOR)
                        HandlePlayer(position, currentPixel, colorData, level);
                    else if (currentPixel == FRIENDLY_COLOR)
                        HandleFriendly(position, currentPixel, colorData, level);
                    else if (currentPixel == ENEMY_COLOR)
                        HandleEnemy(position, currentPixel, colorData, level);
                }
            }
            return level;
        }
        private void HandlePlayer(Point currentPoint, Color dotColor, Color[] colorData, Level level)
        {
            Point position = GetCenterPoint(currentPoint, dotColor, colorData);
            PlayerCell player = new PlayerCell(PlayWindow, PointToPosition(position));
            level.SetPlayer(player);
        }

        private void HandleFriendly(Point currentPoint, Color dotColor, Color[] colorData, Level level)
        {
            Point position = GetCenterPoint(currentPoint, dotColor, colorData);
            GoodCell goodCell = new GoodCell(PlayWindow, PointToPosition(position));
            level.AddFriendly(goodCell);
        }

        private void HandleEnemy(Point currentPoint, Color dotColor, Color[] colorData, Level level)
        {
            Point position = GetCenterPoint(currentPoint, dotColor, colorData);
            Virus virus = new Virus(PlayWindow, PointToPosition(position));
            level.AddEnemy(virus);
        }

        private void HandleWall(Point currentPoint, Color wallColor, Color[] colorData, Level level)
        {
            Vertices vertices = new Vertices();
            // Add each adjacent pixel of the same color as a vertice and then white it
            do
            {
                vertices.Add(PointToPosition(currentPoint));
                colorData[ToArrayIndex(currentPoint.X, currentPoint.Y)] = Color.White;
            } while (GetAdjacent(currentPoint, wallColor, colorData, ref currentPoint));

            if (vertices.Count < 3)
                return;

            Wall.WallType wallType = Wall.WallType.Inner;

            if (wallColor.R % 2 == 0)
                wallType = Wall.WallType.Outer;

            level.AddWall(new Wall(vertices, wallType, PlayWindow));
        }

        private bool GetAdjacent(Point center, Color wallColor, Color[] colorData, ref Point outPoint)
        {
            bool found = false;

            foreach (var offset in _offsetPoints)
            {
                Point currentPoint = AddPoints(center, offset);
                if (IsInside(currentPoint) && colorData[ToArrayIndex(currentPoint)] == wallColor)
                {
                    outPoint = currentPoint;
                    found = true;
                    break;
                }
            }

            return found;
        }

        private LinkedList<Point> GetAllAdjacent(Point center, Color wallColor, Color[] colorData)
        {
            LinkedList<Point> points = new LinkedList<Point>();

            foreach (var offset in _offsetPoints)
            {
                Point currentPoint = AddPoints(center, offset);
                if (IsInside(currentPoint) && colorData[ToArrayIndex(currentPoint)] == wallColor)
                {
                    points.AddFirst(currentPoint);
                }
            }

            return points;
        }

        private Point GetCenterPoint(Point firstPoint, Color pointColor, Color[] colorData)
        {
            Stack<Point> points = new Stack<Point>();
            Point upperLeft, lowerRight;
            
            points.Push(firstPoint);
            colorData[ToArrayIndex(firstPoint)] = Color.White;
            upperLeft = lowerRight = firstPoint;

            while (points.Count > 0)
            {
                Point currentPoint = points.Pop();
                Point adjacentPoint = Point.Zero;

                if (currentPoint.X < upperLeft.X)
                    upperLeft.X = currentPoint.X;
                else if (currentPoint.X > lowerRight.X)
                    lowerRight.X = currentPoint.X;

                if (currentPoint.Y < upperLeft.Y)
                    upperLeft.Y = currentPoint.Y;
                else if (currentPoint.Y > lowerRight.Y)
                    lowerRight.Y = currentPoint.Y;

                foreach (var point in GetAllAdjacent(currentPoint, pointColor, colorData))
                {
                    points.Push(point);
                    colorData[ToArrayIndex(point)] = Color.White;
                }
            }

            Point centerPoint;
            centerPoint.X = (upperLeft.X + lowerRight.X) / 2;
            centerPoint.Y = (upperLeft.Y + lowerRight.Y) / 2;
            return centerPoint;
        }

        private float AdjustX(int x)
        {
            return (x - _imageWidth / 2) * _levelScaleFactor;
        }

        private float AdjustY(int y)
        {
            return (y - _imageHeight / 2) * _levelScaleFactor;
        }

        private int ToArrayIndex(int x, int y)
        {
            return y * _imageWidth + x;
        }

        private int ToArrayIndex(Point point)
        {
            return point.Y * _imageWidth + point.X;
        }

        private Vector2 PointToPosition(Point point)
        {
            return new Vector2(AdjustX(point.X), AdjustY(point.Y));
        }

        private bool IsInside(Point point)
        {
            if (point.X > 0 && point.X < _imageWidth && point.Y > 0 && point.Y < _imageHeight)
                return true;
            else
                return false;
        }

        private static Point AddPoints(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
    }
}
