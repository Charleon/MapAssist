﻿/**
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

namespace MapAssist.Drawing
{
    public static class IconDrawer
    {
        public static Bitmap GenerateIcon(PointOfInterestRendering poiSettings, int sizeMultiplier)
        {
            var iconSize = poiSettings.IconSize * sizeMultiplier;
            var bitmap = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                switch (poiSettings.IconShape)
                {
                    case Shape.Ellipse:
                        g.FillEllipse(new SolidBrush(poiSettings.IconColor), 0, 0, iconSize,
                            iconSize);
                        break;
                    case Shape.Rectangle:
                        g.FillRectangle(new SolidBrush(poiSettings.IconColor), 0, 0, iconSize,
                            iconSize);
                        break;
                }
            }
            return bitmap;
        }
    }
}
