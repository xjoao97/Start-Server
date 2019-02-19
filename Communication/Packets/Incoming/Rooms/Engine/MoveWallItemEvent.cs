#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class MoveWallItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session))
                return;

            var itemID = Packet.PopInt();
            var wallPositionData = Packet.PopString();

            var Item = Room.GetRoomItemHandler().GetItem(itemID);

            if (Item == null)
                return;

            try
            {
                var WallPos = Room.GetRoomItemHandler().WallPositionCheck(":" + wallPositionData.Split(':')[1]);
                Item.wallCoord = WallPos;
            }
            catch
            {
                return;
            }

            Room.GetRoomItemHandler().UpdateItem(Item);
            Room.SendMessage(new ItemUpdateComposer(Item, Room.OwnerId));
        }
    }
}