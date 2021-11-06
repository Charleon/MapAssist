using MapAssist.Helpers;
using MapAssist.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapAssist.Types
{
    public class Room : IUpdatable<Room>
    {
        //needs to be public so it can be accessed from GameMemory for performance reasons
        public IntPtr _pRoom = IntPtr.Zero;
        private Structs.Room _room;

        public Room(IntPtr pRoom)
        {
            _pRoom = pRoom;
            Update();
        }

        public Room Update()
        {
            using (var processContext = GameManager.GetProcessContext())
            {
                _room = processContext.Read<Structs.Room>(_pRoom);
            }
            return this;
        }

        public Room[] RoomsNear
        {
            get
            {
                using (var processContext = GameManager.GetProcessContext())
                {
                    var pRooms = processContext.Read<IntPtr>(_room.pRoomsNear, (int)NumRoomsNear);
                    return pRooms.Select(pRoom => new Room(pRoom)).ToArray();
                }
            }
        }
        public RoomEx RoomEx { get => new RoomEx(_room.pRoomEx);  }
        public uint NumRoomsNear { get => _room.numRoomsNear; }
        public Act Act { get => new Act(_room.pAct);  }
        public UnitAny UnitFirst { get => new UnitAny(_room.pUnitFirst); }
        public Room RoomNext { get => new Room(_room.pRoomNext);  }
    }
}
