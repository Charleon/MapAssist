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

using MapAssist.Helpers;
using MapAssist.Settings;
using MapAssist.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MapAssist.Drawing
{
    public class DrawingBox : PictureBox
    {
        private readonly Timer _timer;
        private readonly MapAssistConfiguration _configuration;
        GameData _gameData;
        AreaData _areaData;
        Bitmap _currentMap;
        IReadOnlyList<PointOfInterest> _pointsOfInterest;
        public DrawingBox(MapAssistConfiguration configuration, GameData gameData, Compositor compositor) : base()
        {
            _configuration = configuration;
            _timer = new Timer();
            Initialize();
        }

        private void Initialize()
        {
            ((ISupportInitialize)(this)).BeginInit();

            BackColor = Color.Transparent;
            Location = new Point(0,0);
            Name = "simpleScreen";
            Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            TabIndex = 0;
            TabStop = false;
            Paint += new PaintEventHandler(Draw);

            ((ISupportInitialize)(this)).EndInit();
        }

        public void Load(object sender, EventArgs e)
        {
            _timer.Interval = _configuration.Map.UpdateTime;
            _timer.Tick += UpdateDraw;
            _timer.Start();
        }

        public void FormClosing(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Update loop for logic before Draw is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDraw(object sender, EventArgs e)
        {
            _timer.Stop();
            if (!Visible)
                Visible = true;

            _timer.Start();
            Refresh();
        }

        /// <summary>
        /// Draws to the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Draw(object sender, PaintEventArgs e)
        {
            if (_gameData != null && _areaData.Area == _gameData.Area) 
            {
                var mapLayer = (Bitmap)_currentMap.Clone();
                
                e.Graphics.DrawImageUnscaled(mapLayer, Point.Empty);
            }
            
        }

        public void UpdateGameAndAreaData(GameData gameData, AreaData areaData, IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            _gameData = gameData;
            _areaData = areaData;
            _pointsOfInterest = pointsOfInterest;

            if (_gameData != null && _areaData != null) 
            { 
                (_currentMap, _) = MiniMapDrawer.DrawMiniMapBackground(_areaData, 1, _configuration.MapColors, _pointsOfInterest);
            }
        }
    }
}
