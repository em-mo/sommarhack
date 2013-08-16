using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SommarFenomen.Objects;
using FarseerPhysics.Common;

namespace SommarFenomen.Level
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

            _levelScaleFactor = 50;
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

                    //Wall if all are the same
                    if (currentPixel.R == currentPixel.G && currentPixel.G == currentPixel.B)
                        HandleWall(x, y, currentPixel, colorData, level);
                    else if (currentPixel == PLAYER_COLOR)
                        HandlePlayer(x, y, level);
                    else if (currentPixel == FRIENDLY_COLOR)
                        HandleFriendly(x, y, level);
                    else if (currentPixel == ENEMY_COLOR)
                        HandleEnemy(x, y, level);
                }
            }

            return level;
        }

        private void HandlePlayer(int x, int y, Level level)
        {
            Vector2 position = new Vector2((float)x * _levelScaleFactor, (float)y * _levelScaleFactor);
            PlayerCell player = new PlayerCell(PlayWindow, position);
            level.SetPlayer(player);
        }

        private void HandleFriendly(int x, int y, Level level)
        {
            Vector2 position = new Vector2((float)x * _levelScaleFactor, (float)y * _levelScaleFactor);
            GoodCell goodCell = new GoodCell(PlayWindow, position);
            level.AddFriendly(goodCell);
        }

        private void HandleEnemy(int x, int y, Level level)
        {
            Vector2 position = new Vector2((float)x * _levelScaleFactor, (float)y * _levelScaleFactor);
            Virus virus = new Virus(PlayWindow, position);
            level.AddEnemy(virus);
        }

        private void HandleWall(int currentX, int currentY, Color wallColor, Color[] colorData, Level level)
        {
            Vertices vertices = new Vertices();

            Point currentPoint = new Point(currentX, currentY);

            // Add each adjacent pixel of the same color as a vertice and then white it
            do
            {
                vertices.Add(new Vector2((float)currentPoint.X * _levelScaleFactor, (float)currentPoint.Y * _levelScaleFactor));
                colorData[currentPoint.Y * _imageWidth + currentPoint.X] = Color.White;
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

                for (int x = 0; x < 3; x++)
                {
                    if (center.X + x > _imageWidth || center.X + x < 0)
                        continue;

                    if (colorData[y * _imageWidth + x] == wallColor)
                    {
                        outPoint.X = x;
                        outPoint.Y = y;

                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }

            return found;
        }
    }
}
