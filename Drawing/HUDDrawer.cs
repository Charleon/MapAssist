/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/MapAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using MapAssist.Types;
using MapAssist.Settings;
using MapAssist.Cache;
using MapAssist.Helpers;
using System.Windows.Forms;
using System.Numerics;

namespace MapAssist.Drawing
{
    public class HUDDrawer
    {
        int _renderScale;
        AreaData _areaData;
        Point _cropOffset;
        Point _bitmapPlayerPosition;
        MapAssistConfiguration _configuration;

        public HUDDrawer(MapAssistConfiguration configuration)
        {
            _configuration = configuration;
            _renderScale = 2;
        }

        public void DrawHUD(
            Graphics screenGraphics,
            Bitmap mapBackground,
            int backgroundScale,
            AreaData areaData,
            GameData gameData,
            Point cropOffset,
            IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            _renderScale = backgroundScale;
            _areaData = areaData;
            _cropOffset = cropOffset;
            _bitmapPlayerPosition = WorldCoordinatesToMapBitmapPixelCoordinates(GetObjectPositionInWorld(gameData.PlayerPosition));

            using (Graphics mapBackgroundGraphics = Graphics.FromImage(mapBackground))
            {
                DrawPlayer(mapBackgroundGraphics, _bitmapPlayerPosition);
                DrawMonsters(mapBackgroundGraphics);
                DrawDestinationLines(mapBackgroundGraphics, pointsOfInterest);
            }

            if (_configuration.Map.OverlayMode)
            {
                DrawMapToScreenOverlay(screenGraphics, mapBackground, gameData);
                return;
            }

            var rotatedResult = _configuration.Rendering.Rotate ? ImageUtils.RotateImage(mapBackground, 53, true, false, Color.Transparent) : mapBackground;

            var scaledResult = ImageUtils.ResizeImage(rotatedResult,
                    (int)(_configuration.Rendering.Size * _configuration.Rendering.ZoomLevel),
                    (int)(_configuration.Rendering.Size * _configuration.Rendering.ZoomLevel));

            DrawMapToScreenFlat(screenGraphics, scaledResult);
            
            DrawWarningTexts(screenGraphics);

            /*StringDrawer.DrawString(screenGraphics,
                $"Drawing position: {renderingPosition.X} {renderingPosition.Y}",
                24,
                new Point(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2),
                Color.Red);*/
        }

        void DrawMapToScreenFlat(Graphics screenGraphics, Bitmap mapScreen)
        {
            var renderingPosition = GetConfiguredLocation(mapScreen.Size);
            screenGraphics.DrawImage(mapScreen, renderingPosition);
        }

        void DrawMapToScreenOverlay(Graphics screenGraphics, Bitmap mapScreen, GameData gameData)
        {
            float width = 0;
            float height = 0;
            var scale = 0.0F;
            var center = new Vector2();

            switch (_configuration.Rendering.Position)
            {
                case MapPosition.Center:
                    width = Screen.PrimaryScreen.WorkingArea.Width;
                    height = Screen.PrimaryScreen.WorkingArea.Height;
                    scale = (1024.0F / height * width * 3f / 4f / 2.3F) * _configuration.Rendering.ZoomLevel;
                    center = new Vector2(width / 2, height / 2 + 20);

                    screenGraphics.SetClip(new RectangleF(0, 0, width, height));
                    break;
                case MapPosition.TopLeft:
                    width = 640;
                    height = 360;
                    scale = (1024.0F / height * width * 3f / 4f / 3.35F) * _configuration.Rendering.ZoomLevel;
                    center = new Vector2(width / 2, (height / 2) + 48);

                    screenGraphics.SetClip(new RectangleF(0, 50, width, height));
                    break;
                case MapPosition.TopRight:
                    width = 640;
                    height = 360;
                    scale = (1024.0F / height * width * 3f / 4f / 3.35F) * _configuration.Rendering.ZoomLevel;
                    center = new Vector2(width / 2, (height / 2) + 40);

                    screenGraphics.TranslateTransform(Screen.PrimaryScreen.WorkingArea.Width - width, -8);
                    screenGraphics.SetClip(new RectangleF(0, 50, width, height));
                    break;
            }

            Point playerPosInArea = gameData.PlayerPosition.OffsetFrom(_areaData.Origin).OffsetFrom(_cropOffset).Multiply(_renderScale);

            var playerPos = new Vector2(playerPosInArea.X, playerPosInArea.Y);

            Vector2 Transform(Vector2 p) =>
                center +
                DeltaInWorldToMinimapDelta(
                    p - playerPos,
                    (float)Math.Sqrt(width * width + height * height),
                    scale,
                    0);

            var p1 = Transform(new Vector2(0, 0));
            var p2 = Transform(new Vector2(mapScreen.Width, 0));
            var p4 = Transform(new Vector2(0, mapScreen.Height));

            PointF[] destinationPoints = {
                    new PointF(p1.X, p1.Y),
                    new PointF(p2.X, p2.Y),
                    new PointF(p4.X, p4.Y)
                };

            screenGraphics.DrawImage(mapScreen, destinationPoints);
        }

        public Vector2 DeltaInWorldToMinimapDelta(Vector2 delta, double diag, float scale, float deltaZ = 0)
        {
            var CAMERA_ANGLE = -26F * 3.14159274F / 180;

            var cos = (float)(diag * Math.Cos(CAMERA_ANGLE) / scale);
            var sin = (float)(diag * Math.Sin(CAMERA_ANGLE) /
                               scale);

            return new Vector2((delta.X - delta.Y) * cos, deltaZ - (delta.X + delta.Y) * sin);
        }

        public Point GetConfiguredLocation(Size mapSize)
        {
            int xOffset = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.05);
            int yOffset = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.1);
            switch (_configuration.Rendering.Position)
            {
                case MapPosition.TopLeft:
                    return new Point(
                        Screen.PrimaryScreen.WorkingArea.X + xOffset,
                        Screen.PrimaryScreen.WorkingArea.Y + yOffset
                        );
                case MapPosition.TopRight:
                    return new Point(
                        Screen.PrimaryScreen.WorkingArea.Width - xOffset - mapSize.Width,
                        Screen.PrimaryScreen.WorkingArea.Y + yOffset
                        );
                case MapPosition.Center:
                    return new Point(
                        Screen.PrimaryScreen.WorkingArea.Width / 2 - mapSize.Width / 2,
                        Screen.PrimaryScreen.WorkingArea.Height / 2 - mapSize.Height / 2
                        );
                default:
                    return new Point(0, 0);

            }
        }

        private Point GetObjectPositionInWorld(Point objectPosition)
        {
            return objectPosition.OffsetFrom(_areaData.Origin);
        }

        private void DrawPlayer(Graphics graphics, Point playerPositionInBitmap)
        {
            if (_configuration.Rendering.Player.CanDrawIcon())
            {
                Bitmap playerIcon = IconCache.GetIcon(_configuration.Rendering.Player, _renderScale);
                graphics.DrawImage(playerIcon, playerPositionInBitmap);
            }
        }

        public void DrawWarningTexts(Graphics graphics)
        {
            var msgCount = 0;
            foreach (var warning in GameMemory.WarningMessages)
            {
                var fontSize = _configuration.Map.WarnImmuneNPCFontSize;
                Font font = FontCache.GetFont(_configuration.Map.WarnImmuneNPCFont, fontSize);
                StringDrawer.DrawString(graphics,
                    warning,
                    font,
                    fontSize,
                    _configuration.Map.WarnNPCHorizontalAlign,
                    _configuration.Map.WarnNPCVerticalAlign,
                    new Point(Screen.PrimaryScreen.WorkingArea.Width / 2, 10 + (msgCount * (fontSize + fontSize / 2))),
                    _configuration.Map.WarnNPCFontColor);
                msgCount++;
            }
        }

        private void DrawDestinationLines(Graphics graphics, IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            foreach (PointOfInterest poi in pointsOfInterest)
            {
                Point poiBitmapPoiPosition = WorldCoordinatesToMapBitmapPixelCoordinates(poi.Position.OffsetFrom(_areaData.Origin));

                if (poi.RenderingSettings.CanDrawLine())
                {
                    var pen = new Pen(poi.RenderingSettings.LineColor, Scale(poi.RenderingSettings.LineThickness));
                    if (poi.RenderingSettings.CanDrawArrowHead())
                    {
                        LineDrawer.DrawLine(graphics,
                            _bitmapPlayerPosition,
                            poiBitmapPoiPosition,
                            poi.RenderingSettings.LineColor,
                            Scale(poi.RenderingSettings.LineThickness),
                            Scale(poi.RenderingSettings.ArrowHeadSize));
                    }
                    else
                    {
                        LineDrawer.DrawLine(graphics,
                            _bitmapPlayerPosition,
                            poiBitmapPoiPosition,
                            poi.RenderingSettings.LineColor,
                            Scale(poi.RenderingSettings.LineThickness));
                    }
                }
            }
        }

        private float Scale(float input)
        {
            return input * _renderScale;
        }

        private int Scale(int input)
        {
            return input * _renderScale;
        }

        private void DrawMonsters(Graphics graphics)
        {
            MobRendering render = Utils.GetMobRendering();
            foreach (var monster in GameMemory.Monsters)
            {
                var clr = monster.UniqueFlag == 0 ? render.NormalColor : render.UniqueColor;
                var pen = new Pen(clr, 1);
                var sz = new Size(Scale(5), Scale(5));
                var sz2 = new Size(Scale(2), Scale(2));
                var midPoint = monster.Position.OffsetFrom(_areaData.Origin);
                var bitmapMidPoint = WorldCoordinatesToMapBitmapPixelCoordinates(midPoint);
                var rect = new Rectangle(bitmapMidPoint, sz);
                graphics.DrawRectangle(pen, rect);
                var i = 0;
                foreach (var immunity in monster.Immunities)
                {
                    var brush = new SolidBrush(ResistColors.ResistColor[immunity]);
                    var iPoint = new Point((i * -2) + (1 * (monster.Immunities.Count - 1)) - 1, 3);
                    var pen2 = new Pen(ResistColors.ResistColor[immunity], 1);
                    var rect2 = new Rectangle(bitmapMidPoint.OffsetFrom(iPoint), sz2);
                    graphics.FillRectangle(brush, rect2);
                    i++;
                }
            }
        }

        private Point WorldCoordinatesToMapBitmapPixelCoordinates(Point worldObjectCoordinate)
        {
            var positionAfterCrop = worldObjectCoordinate.OffsetFrom(_cropOffset);
            var scaledPosition = new Point(Scale(positionAfterCrop.X), Scale(positionAfterCrop.Y));
            return scaledPosition;
        }
    }
}
