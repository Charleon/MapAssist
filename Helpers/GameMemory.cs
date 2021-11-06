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
using System.Diagnostics;
using MapAssist.Types;

namespace MapAssist.Helpers
{
    public static class GameMemory
    {

        //the convenience functions added are nice, but simply have too much overhead for a recursive method like this
        //it seems the problem stems from the using...GetProcessContext() wrapper, because it's opening and closing a new handle every time you ask for info about a room, unit, etc
        //so the below code isn't pretty, but it's fast.  if you want to stick to this new code pattern i think some structure needs to change
        //ie. grab one handle and use that for an entire read cycle (update tick) and then dispose it, rather than opening 500 handles every cycle
        private static HashSet<IntPtr> GetRooms1(ref HashSet<IntPtr> roomList, IntPtr roomAddress)
        {
            using (var processContext = GameManager.GetProcessContext())
            {
                var addressBuffer = new byte[8];
                var dwordBuffer = new byte[4];
                WindowsExternal.ReadProcessMemory((IntPtr)processContext.Handle, roomAddress, addressBuffer, addressBuffer.Length, out _);
                var aRoomNear = (IntPtr)BitConverter.ToInt64(addressBuffer, 0);
                var pRoomNext = IntPtr.Add(roomAddress, 0xB0);
                WindowsExternal.ReadProcessMemory((IntPtr)processContext.Handle, pRoomNext, addressBuffer, addressBuffer.Length, out _);
                var aRoomNext = (IntPtr)BitConverter.ToInt64(addressBuffer, 0);
                if (aRoomNear != IntPtr.Zero)
                {
                    var pRoomsNear = IntPtr.Add(roomAddress, 0x40);
                    WindowsExternal.ReadProcessMemory((IntPtr)processContext.Handle, pRoomsNear, dwordBuffer, dwordBuffer.Length, out _);
                    var roomsNear = BitConverter.ToUInt32(dwordBuffer, 0);
                    for (var i = 0; i < roomsNear; i++)
                    {
                        var pRoomToRead = IntPtr.Add(aRoomNear, i * 8);
                        WindowsExternal.ReadProcessMemory((IntPtr)processContext.Handle, pRoomToRead, addressBuffer, addressBuffer.Length, out _);
                        var aRoomToRead = (IntPtr)BitConverter.ToInt64(addressBuffer, 0);
                        if (!roomList.Contains(aRoomToRead))
                        {
                            roomList.Add(aRoomToRead);
                            GetRooms1(ref roomList, aRoomToRead);
                        }
                    }
                }
                if (aRoomNext != IntPtr.Zero)
                {
                    if (!roomList.Contains(aRoomNext))
                    {
                        roomList.Add(aRoomNext);
                        GetRooms1(ref roomList, aRoomNext);
                    }
                }
            }
            return roomList;
        }
        //this is how i would do it with the new code pattern but it is way too slow
        /*private static HashSet<IntPtr> GetRooms(Room startingRoom, ref HashSet<IntPtr> roomsList)
        {
            var roomsNear = startingRoom.RoomsNear;
            foreach (var roomNear in roomsNear)
            {
                if (!roomsList.Contains(roomNear._pRoom))
                {
                    roomsList.Add(roomNear._pRoom);
                    GetRooms(roomNear, ref roomsList);
                }
            }
            if (!roomsList.Contains(startingRoom.RoomNext._pRoom))
            {
                roomsList.Add(startingRoom.RoomNext._pRoom);
                GetRooms(startingRoom.RoomNext, ref roomsList);
            }
            return roomsList;
        }*/
        public static GameData GetGameData()
        {
            try
            {
                var playerUnit = GameManager.PlayerUnit;
                playerUnit.Update();

                var mapSeed = playerUnit.Act.MapSeed;

                if (mapSeed <= 0 || mapSeed > 0xFFFFFFFF)
                {
                    throw new Exception("Map seed is out of bounds.");
                }

                var actId = playerUnit.Act.ActId;

                var gameDifficulty = playerUnit.Act.ActMisc.GameDifficulty;

                if (!gameDifficulty.IsValid())
                {
                    throw new Exception("Game difficulty out of bounds.");
                }

                var levelId = playerUnit.Path.Room.RoomEx.Level.LevelId;

                if (!levelId.IsValid())
                {
                    throw new Exception("Level id out of bounds.");
                }

                var mapShown = GameManager.UiSettings.MapShown;

                var items = new List<Types.UnitAny>();
                var monsters = new List<Types.UnitAny>();
                var players = new List<Types.UnitAny>();
                var shrines = new List<Types.UnitAny>();
                var objects = new List<Types.UnitAny>();

                var rooms = new HashSet<IntPtr>() { playerUnit.Path.Room._pRoom };
                //rooms = GetRooms(playerUnit.Path.Room, ref rooms);
                rooms = GetRooms1(ref rooms, playerUnit.Path.Room._pRoom);
                foreach(var roomptr in rooms)
                {
                    var room = new Room(roomptr);
                    var unit = room.UnitFirst;
                    while (unit.IsValid())
                    {
                        //don't need to call Update() because unit is updated automatically when you grab room.UnitFirst
                        //unit.Update();
                        switch (unit.UnitType)
                        {
                            case UnitType.Object:
                                objects.Add(unit);
                                break;
                            case UnitType.Monster:
                                if (unit.IsMonster())
                                {
                                    monsters.Add(unit);
                                }
                                break;
                            case UnitType.Player:
                                players.Add(unit);
                                break;
                            case UnitType.Item:
                                items.Add(unit);
                                break;
                        }
                        unit = unit.RoomNext;
                    }
                }

                return new GameData
                {
                    PlayerPosition = playerUnit.Position,
                    MapSeed = mapSeed,
                    Area = levelId,
                    Difficulty = gameDifficulty,
                    MapShown = mapShown,
                    MainWindowHandle = GameManager.MainWindowHandle,
                    Items = items,
                    Players = players,
                    Monsters = monsters,
                    Objects = objects,
                    Shrines = shrines,
                    PlayerName = playerUnit.Name
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                GameManager.ResetPlayerUnit();
                return null;
            }
        }
    }
}
