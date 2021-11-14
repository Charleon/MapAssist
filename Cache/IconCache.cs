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
using MapAssist.Drawing;

namespace MapAssist.Cache
{
    public static class IconCache
    {
        private static readonly Dictionary<(Shape, int, Color), Bitmap> _iconCache =
            new Dictionary<(Shape, int, Color), Bitmap>();
        
        public static Bitmap GetIcon(PointOfInterestRendering poiSettings, int sizeMultiplier)
        {
            var iconSize = poiSettings.IconSize * sizeMultiplier;
            (Shape IconShape, int IconSize, Color Color) cacheKey = (poiSettings.IconShape, iconSize, Color: poiSettings.IconColor);
            if (!_iconCache.ContainsKey(cacheKey))
            {
                var bitmap = IconDrawer.GenerateIcon(poiSettings, sizeMultiplier);

                _iconCache[cacheKey] = bitmap;
            }

            return _iconCache[cacheKey];
        }
    }
}
