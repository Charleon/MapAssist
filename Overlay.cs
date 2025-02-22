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
using System.Windows.Forms;
using MapAssist.Types;
using MapAssist.Helpers;
using MapAssist.Settings;
using Gma.System.MouseKeyHook;
using System.Numerics;
using System.Configuration;
using System.Diagnostics;

namespace MapAssist
{
    public partial class Overlay : Form
    {
        // Move to windows external
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private readonly Timer _timer = new Timer();
        private GameData _currentGameData;
        private AreaData _areaData;
        private MapApi _mapApi;
        private bool _show = true;
        private Screen _screen;
        private MapAssistConfiguration _configuration;

        public Overlay(IKeyboardMouseEvents keyboardMouseEvents, MapAssistConfiguration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            keyboardMouseEvents.KeyPress += (_, args) =>
            {
                if (InGame())
                {
                    if (args.KeyChar == _configuration.Map.ToggleKey)
                    {
                        _show = !_show;
                    }
                    if (args.KeyChar == _configuration.Map.ZoomInKey)
                    {
                        _configuration.Rendering.ZoomLevel += 0.25f;
                    }
                    if (args.KeyChar == _configuration.Map.ZoomOutKey)
                    {
                        _configuration.Rendering.ZoomLevel -= 0.25f;
                    }

                    _configuration.Rendering.ZoomLevel = Math.Min(4f, _configuration.Rendering.ZoomLevel);
                    _configuration.Rendering.ZoomLevel = Math.Max(0.25f, _configuration.Rendering.ZoomLevel);
                }
            };
        }

        private void Overlay_Load(object sender, EventArgs e)
        {
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            var width = Width >= screen.Width ? screen.Width : (screen.Width + Width) / 2;
            var height = Height >= screen.Height ? screen.Height : (screen.Height + Height) / 2;
            Location = new Point((screen.Width - width) / 2, (screen.Height - height) / 2);
            Size = new Size(width, height);
            Opacity = _configuration.Map.Opacity;

            _timer.Interval = _configuration.Map.UpdateTime;
            _timer.Tick += MapUpdateTimer_Tick;
            _timer.Start();

            if (_configuration.Map.AlwaysOnTop) SetTopMost();
        }

        private void Overlay_FormClosing(object sender, EventArgs e)
        {
            _mapApi?.Dispose();
        }

        private void MapUpdateTimer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            GameData gameData = GameMemory.GetGameData(_configuration);

            if (gameData != null)
            {
                if (gameData.HasGameChanged(_currentGameData))
                {
                    Console.WriteLine($"Game changed: {gameData}");
                    _mapApi?.Dispose();
                    _mapApi = new MapApi(gameData.Difficulty, gameData.MapSeed, _configuration);
                }

                if (gameData.HasMapChanged(_currentGameData))
                {
                    Debug.WriteLine($"Area changed: {gameData.Area}");
                    if (gameData.Area != Area.None)
                    {
                        _areaData = _mapApi.GetMapData(gameData.Area);
                    }
                }
            }

            _currentGameData = gameData;

            if (_mapApi != null && _areaData != null)
            {
                List<PointOfInterest> pointsOfInterest = PointOfInterestHandler.Get(_mapApi, _areaData, _configuration);
                _overlayDrawer.UpdateGameAndAreaData(_currentGameData, _areaData, pointsOfInterest);
            }

            if (ShouldHideMap())
            {
                _overlayDrawer.Hide();
            }
            else
            {
                if (!_overlayDrawer.Visible)
                {
                    _overlayDrawer.Show();
                    if (_configuration.Map.AlwaysOnTop) SetTopMost();
                }
                _overlayDrawer.Refresh();
            }

            _timer.Start();
        }

        private void SetTopMost()
        {
            var initialStyle = (uint)WindowsExternal.GetWindowLongPtr(Handle, -20);
            WindowsExternal.SetWindowLong(Handle, -20, initialStyle | 0x80000 | 0x20);
            WindowsExternal.SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        private bool ShouldHideMap()
        {
            if (!_show) return true;
            if (!InGame()) return true;
            if (_currentGameData.Area == Area.None) return true;
            if (Array.Exists(_configuration.Map.HiddenAreas, element => element == _currentGameData.Area)) return true;
            if (_configuration.Map.ToggleViaInGameMap && !_currentGameData.MapShown) return true;
            return false;
        }

        private bool InGame()
        {
            return _currentGameData != null &&
                _currentGameData.MainWindowHandle != IntPtr.Zero &&
                 WindowsExternal.GetForegroundWindow() == _currentGameData.MainWindowHandle;
        }

        private void MapOverlay_Paint(object sender, PaintEventArgs e)
        {
            UpdateLocation();
        }

        /// <summary>
        /// Update the location and size of the form relative to the window location.
        /// </summary>
        private void UpdateLocation()
        {
            _screen = Screen.FromHandle(_currentGameData.MainWindowHandle);
            Location = new Point(_screen.WorkingArea.X, _screen.WorkingArea.Y);
            Size = new Size(_screen.WorkingArea.Width, _screen.WorkingArea.Height);
            //mapOverlay.Size = Size;
        }
    }
}
