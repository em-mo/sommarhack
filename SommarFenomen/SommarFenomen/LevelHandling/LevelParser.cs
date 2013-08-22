﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects;
using FarseerPhysics.Common;

namespace SommarFenomen.LevelHandling
{
    class LevelParser
    {
        #region ColorDefines
        private static readonly Color PLAYER_COLOR = new Color(0, 255, 0);
        private static readonly Color FRIENDLY_COLOR = new Color(0, 0, 255);
        private static readonly Color ENEMY_COLOR = new Color(255, 0, 0);

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
                        HandlePlayer(position, level);
                    else if (currentPixel == FRIENDLY_COLOR)
                        HandleFriendly(position, level);
                    else if (currentPixel == ENEMY_COLOR)
                        HandleEnemy(position, level);
                }
            }

            return level;
        }

        private void HandlePlayer(Point position, Level level)
        {
            PlayerCell player = new PlayerCell(PlayWindow, PointToPosition(position));
            level.SetPlayer(player);
        }

        private void HandleFriendly(Point position, Level level)
        {
            GoodCell goodCell = new GoodCell(PlayWindow, PointToPosition(position));
            level.AddFriendly(goodCell);
        }

        private void HandleEnemy(Point position, Level level)
        {
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
            
            Wall.WallType wallType = Wall.WallType.Inner;

            if (wallColor.R % 2 == 0)
                wallType = Wall.WallType.Outer;

            level.AddWall(new Wall(vertices, wallType, PlayWindow));
        }

        /// <summary>
        /// Gets the coordinate of an adjacent wallpixel.
        /// Returns null if none is found.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="colorData"></param>
        /// <returns>Null if not found</returns>
        private bool GetAdjacent(Point center, Color wallColor, Color[] colorData, ref Point outPoint)
        {
            bool found = false;
            for (int y = -1; y < 2; y++)
            {
                if (center.Y + y > _imageHeight || center.Y + y < 0)
                    continue;

                for (int x = -1; x < 2; x++)
                {
                    if (center.X + x > _imageWidth || center.X + x < 0)
                        continue;

                    if (colorData[ToArrayIndex(center.X + x, center.Y + y)] == wallColor)
                    {
                        outPoint.X = center.X + x;
                        outPoint.Y = center.Y + y;

                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }

            return found;
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

        private Vector2 PointToPosition(Point point)
        {
            return new Vector2(AdjustX(point.X), AdjustY(point.Y));
        }
    }
}