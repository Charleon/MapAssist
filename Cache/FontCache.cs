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
    public static class FontCache
    {
        private static readonly Dictionary<(string, int), Font> _fontCache = new Dictionary<(string, int), Font>();

        public static Font GetFont(PointOfInterestRendering poiSettings, int sizeMultiplier)
        {
            int fontSize = poiSettings.LabelFontSize * sizeMultiplier;
            (string LabelFont, int LabelFontSize) cacheKey = (poiSettings.LabelFont, fontSize);
            if (!_fontCache.ContainsKey(cacheKey))
            {
                var font = new Font(poiSettings.LabelFont,
                    fontSize);
                _fontCache[cacheKey] = font;
            }

            return _fontCache[cacheKey];
        }

        public static Font GetFont(string fontStr, int fontSize)
        {
            (string LabelFont, int LabelFontSize) cacheKey = (fontStr, fontSize);
            if (!_fontCache.ContainsKey(cacheKey))
            {
                var font = new Font(fontStr,
                    fontSize);
                _fontCache[cacheKey] = font;
            }

            return _fontCache[cacheKey];
        }
    }
}
