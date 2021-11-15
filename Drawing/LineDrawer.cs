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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using MapAssist.Settings;

namespace MapAssist.Drawing
{
    public static class LineDrawer
    {
        public static void DrawLine(Graphics graphics,
            Point from,
            Point to,
            Color lineColor,
            float lineThickness,
            float arrowHeadSize
            )
        {
            var pen = new Pen(lineColor, lineThickness);

            pen.CustomEndCap = new AdjustableArrowCap(arrowHeadSize, arrowHeadSize);

            graphics.DrawLine(pen, from, to);
        }

        public static void DrawLine(Graphics graphics,
            Point from,
            Point to,
            Color lineColor,
            float lineThickness)
        {
            var pen = new Pen(lineColor, lineThickness);

            graphics.DrawLine(pen, from, to);
        }
    }
}
