#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    internal class DeleteStickyNoteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session))
                return;

            var Item = Room.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Item == null)
                return;

            if (Item.GetBaseItem().InteractionType == InteractionType.Postit ||
                Item.GetBaseItem().InteractionType == InteractionType.CameraPicture)
            {
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                }
            }
        }
    }
}