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

namespace MapAssist.Drawing
{
    public static class StringDrawer
    {
        public static void DrawString(Graphics graphics, 
            string text, 
            Font font,
            int fontSize, 
            StringAlignment horizontalAlign, 
            StringAlignment verticalAlign, 
            Point location,
            Color color)
        {
            
            var stringFormat = new StringFormat();
            stringFormat.Alignment = horizontalAlign;
            stringFormat.LineAlignment = verticalAlign;
            
            graphics.DrawString(text, font,
                new SolidBrush(color),
                location, 
                stringFormat);
        }

        public static void DrawString(Graphics graphics,
            string text,
            int fontSize,
            Point location,
            Color color)
        {

            var stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            Font font = FontCache.GetFont("Arial", fontSize);

            graphics.DrawString(text, font,
                new SolidBrush(color),
                location,
                stringFormat);
        }
    }
}
