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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using MapAssist.Structs;
using MapAssist.Types;

namespace MapAssist.Helpers
{
    class GameMemory
    {
        private static readonly string ProcessName = Encoding.UTF8.GetString(new byte[] { 68, 50, 82 });
        private static UnitAny PlayerUnit = default;
        private static int _lastProcessId = 0;
        private static IntPtr PlayerUnitPtr;
        private static Difficulty currentDifficulty = Difficulty.None;
        private static uint currentMapSeed = 0;

        unsafe public static GameData GetGameData()
        {
            IntPtr processHandle = IntPtr.Zero;

            try
            {
                Process[] process = Process.GetProcessesByName(ProcessName);

                Process gameProcess = null;

                IntPtr windowInFocus = WindowsExternal.GetForegroundWindow();
                if (windowInFocus == IntPtr.Zero)
                {
                    gameProcess = process.FirstOrDefault();
                }
                else
                {
                    gameProcess = process.FirstOrDefault(p => p.MainWindowHandle == windowInFocus);
                }

                if (gameProcess == null)
                {
                    throw new Exception("Game process not found.");
                }

                // If changing processes we need to re-find the player
                if (gameProcess.Id != _lastProcessId)
                {
                    ResetPlayerUnit();
                }

                _lastProcessId = gameProcess.Id;

                processHandle =
                    WindowsExternal.OpenProcess((uint)WindowsExternal.ProcessAccessFlags.VirtualMemoryRead, false, gameProcess.Id);
                IntPtr processAddress = gameProcess.MainModule.BaseAddress;

                if (PlayerUnitPtr == IntPtr.Zero)
                {
                    var expansionCharacter = Read<byte>(processHandle, IntPtr.Add(processAddress, Offsets.ExpansionCharacter)) == 1;
                    var userBaseOffset = 0x30;
                    var checkUser1 = 1;
                    if (expansionCharacter)
                    {
                        userBaseOffset = 0x70;
                        checkUser1 = 0;
                    }
                    var unitHashTable = Read<UnitHashTable>(processHandle, IntPtr.Add(processAddress, Offsets.UnitHashTable));
                    foreach (var pUnitAny in unitHashTable.UnitTable)
                    {
                        var pListNext = pUnitAny;

                        while (pListNext != IntPtr.Zero)
                        {
                            var unitAny = Read<UnitAny>(processHandle, pListNext);
                            if (unitAny.Inventory != IntPtr.Zero)
                            {
                                var UserBaseCheck = Read<int>(processHandle, IntPtr.Add(unitAny.Inventory, userBaseOffset));
                                if (UserBaseCheck != checkUser1)
                                {
                                    PlayerUnitPtr = pUnitAny;
                                    PlayerUnit = unitAny;
                                    break;
                                }
                            }
                            pListNext = (IntPtr)unitAny.pListNext;
                        }

                        if (PlayerUnitPtr != IntPtr.Zero)
                        {
                            break;
                        }
                    }
                }

                if (PlayerUnitPtr == IntPtr.Zero)
                {
                    currentDifficulty = Difficulty.None;
                    currentMapSeed = 0;
                    throw new Exception("Player pointer is zero.");
                }

                var playerName = Encoding.ASCII.GetString(Read<byte>(processHandle, PlayerUnit.UnitData, 16)).TrimEnd((char)0);
                var act = Read<Act>(processHandle, (IntPtr)PlayerUnit.pAct);
                if (currentMapSeed == 0)
                {
                    currentMapSeed = act.MapSeed;
                }
                var mapSeed = currentMapSeed;

                if (mapSeed <= 0 || mapSeed > 0xFFFFFFFF)
                {
                    throw new Exception("Map seed is out of bounds.");
                }

                var actId = act.ActId;
                var actMisc = Read<ActMisc>(processHandle, (IntPtr)act.ActMisc);
                if (currentDifficulty == Difficulty.None)
                {
                    currentDifficulty = actMisc.GameDifficulty;
                }
                var gameDifficulty = currentDifficulty;

                if (!gameDifficulty.IsValid())
                {
                    throw new Exception("Game difficulty out of bounds.");
                }

                var path = Read<Path>(processHandle, (IntPtr)PlayerUnit.pPath);
                var positionX = path.DynamicX;
                var positionY = path.DynamicY;
                var room = Read<Room>(processHandle, (IntPtr)path.pRoom);
                var roomEx = Read<RoomEx>(processHandle, (IntPtr)room.pRoomEx);
                var level = Read<Level>(processHandle, (IntPtr)roomEx.pLevel);
                var levelId = level.LevelId;

                if (!levelId.IsValid())
                {
                    throw new Exception("Level id out of bounds.");
                }

                var mapShown = Read<UiSettings>(processHandle, IntPtr.Add(processAddress, Offsets.UiSettings)).MapShown == 1;

                return new GameData
                {
                    PlayerPosition = new Point(positionX, positionY),
                    MapSeed = mapSeed,
                    Area = levelId,
                    Difficulty = gameDifficulty,
                    MapShown = mapShown,
                    MainWindowHandle = gameProcess.MainWindowHandle,
                    PlayerName = playerName
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                ResetPlayerUnit();
                return null;
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    WindowsExternal.CloseHandle(processHandle);
                }
            }
        }

        private static void ResetPlayerUnit()
        {
            PlayerUnit = default;
            PlayerUnitPtr = IntPtr.Zero;
        }

        public static T[] Read<T>(IntPtr processHandle, IntPtr address, int count) where T : struct
        {
            var sz = Marshal.SizeOf<T>();
            var buf = new byte[sz * count];
            WindowsExternal.ReadProcessMemory(processHandle, address, buf, buf.Length, out _);

            var handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
            try
            {
                var result = new T[count];
                for (var i = 0; i < count; i++)
                {
                    result[i] = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject() + (i * sz), typeof(T));
                }

                return result;
            }
            finally
            {
                handle.Free();
            }
        }

        public static T Read<T>(IntPtr processHandle, IntPtr address) where T : struct
        {
            return Read<T>(processHandle, address, 1)[0];
        }
    }
}
