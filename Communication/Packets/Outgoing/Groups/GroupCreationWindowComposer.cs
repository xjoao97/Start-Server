#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class GroupCreationWindowComposer : ServerPacket
    {
        public GroupCreationWindowComposer(ICollection<RoomData> Rooms)
            : base(ServerPacketHeader.GroupCreationWindowMessageComposer)
        {
            WriteInteger(OblivionStaticGameSettings.GroupPurchaseAmount); //Price

            WriteInteger(Rooms.Count); //Room count that the user has.
            foreach (var Room in Rooms)
            {
                WriteInteger(Room.Id); //Room Id
                WriteString(Room.Name); //Room Name
                WriteBoolean(false); //What?
            }

            WriteInteger(5);
            WriteInteger(5);
            WriteInteger(11);
            WriteInteger(4);

            WriteInteger(6);
            WriteInteger(11);
            WriteInteger(4);

            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);

            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);

            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
        }
    }
}