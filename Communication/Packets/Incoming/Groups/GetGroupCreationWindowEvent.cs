#region

using System.Collections.Generic;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class GetGroupCreationWindowEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
                return;

            var ValidRooms = new List<RoomData>();
            foreach (var Data in Session.GetHabbo().UsersRooms)
                if (Data.Group == null)
                    ValidRooms.Add(Data);

            Session.SendMessage(new GroupCreationWindowComposer(ValidRooms));
        }
    }
}