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

namespace MapAssist.Drawing
{
    public abstract class MiniMapDrawer
    {
        public static (Bitmap, Point) DrawMiniMapBackground(AreaData areaData,
            int sizeMultiplier,
            MapColorConfiguration configuration,
            IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            var background = new Bitmap(areaData.CollisionGrid[0].Length, areaData.CollisionGrid.Length,
                PixelFormat.Format32bppArgb);

            using (Graphics backgroundGraphics = Graphics.FromImage(background))
            {
                // The transparent space that's left will be cropped later
                backgroundGraphics.FillRectangle(new SolidBrush(Color.Transparent), 0, 0,
                    areaData.CollisionGrid[0].Length,
                    areaData.CollisionGrid.Length);

                backgroundGraphics.CompositingQuality = CompositingQuality.HighQuality;
                backgroundGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                backgroundGraphics.SmoothingMode = SmoothingMode.HighQuality;
                backgroundGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                for (var y = 0; y < areaData.CollisionGrid.Length; y++)
                {
                    for (var x = 0; x < areaData.CollisionGrid[y].Length; x++)
                    {
                        int type = areaData.CollisionGrid[y][x];
                        Color? typeColor = configuration.LookupMapColor(type);
                        if (typeColor != null)
                        {
                            background.SetPixel(x, y, (Color)typeColor);
                        }
                    }
                }

                var scaled = new Bitmap(background,
                    new Size(
                        new Point(
                        sizeMultiplier * background.Size.Width,
                        sizeMultiplier * background.Size.Height)
                        )
                    );

                using (Graphics mapGraphics = Graphics.FromImage(scaled))
                {
                    foreach (PointOfInterest poi in pointsOfInterest)
                    {
                        Point offset = new Point(poi.Position.OffsetFrom(areaData.Origin).X * sizeMultiplier, poi.Position.OffsetFrom(areaData.Origin).Y * sizeMultiplier);
                        if (poi.RenderingSettings.CanDrawIcon())
                        {
                            Bitmap icon = IconCache.GetIcon(poi.RenderingSettings, sizeMultiplier);
                            Point drawPosition = new Point(offset.X - icon.Size.Width / 2, offset.Y - icon.Size.Height / 2);
                            mapGraphics.DrawImage(icon, drawPosition);
                        }

                        if (!string.IsNullOrWhiteSpace(poi.Label) && poi.RenderingSettings.CanDrawLabel())
                        {
                            Font font = FontCache.GetFont(poi.RenderingSettings, sizeMultiplier);
                            mapGraphics.DrawString(poi.Label, font,
                                new SolidBrush(poi.RenderingSettings.LabelColor),
                                offset);
                        }
                    }
                }
                var cropped = ImageUtils.CropBitmap(scaled);
                return cropped;
            }
        }
    }
}
