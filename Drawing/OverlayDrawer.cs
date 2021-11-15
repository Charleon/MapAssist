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

using MapAssist.Settings;
using MapAssist.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MapAssist.Drawing
{
    public class OverlayDrawer : PictureBox
    {
        private readonly Timer _timer;
        private readonly MapAssistConfiguration _configuration;
        GameData _gameData;
        AreaData _areaData;
        Bitmap _currentMap;
        int _generatedMapScale;
        Point _offsetAfterCrop;
        Area _currentArea;
        IReadOnlyList<PointOfInterest> _pointsOfInterest;
        HUDDrawer _hudDrawer;
        public OverlayDrawer(MapAssistConfiguration configuration) : base()
        {
            _configuration = configuration;
            _timer = new Timer();
            _hudDrawer = new HUDDrawer(configuration);
            Initialize();
        }

        private void Initialize()
        {
            ((ISupportInitialize)(this)).BeginInit();

            BackColor = Color.Transparent;
            Location = new Point(0, 0);
            Name = "GameScreen";
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
            _timer.Stop();
        }

        /// <summary>
        /// Update loop for logic before Draw is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDraw(object sender, EventArgs e)
        {
            _timer.Stop();

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

                _hudDrawer.DrawHUD(
                    e.Graphics,
                    mapLayer,
                    _generatedMapScale,
                    _areaData,
                    _gameData,
                    _offsetAfterCrop,
                    _pointsOfInterest);
            }
        }

        public void UpdateGameAndAreaData(GameData gameData, AreaData areaData, IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            _gameData = gameData;
            _areaData = areaData;
            _pointsOfInterest = pointsOfInterest;

            if (_gameData != null && _areaData != null)
            {
                // Only Regenerate map if we have switched map.
                if (_currentArea != areaData.Area)
                {
                    ((_currentMap, _offsetAfterCrop), _generatedMapScale) = MiniMapDrawer.DrawMiniMapBackground(_areaData,  _configuration.MapColors, _pointsOfInterest, _configuration.Rendering.Size);
                    _offsetAfterCrop = new Point(_offsetAfterCrop.X / _generatedMapScale, _offsetAfterCrop.Y / _generatedMapScale);
                    _currentArea = areaData.Area;
                }
            }
        }
    }
}
