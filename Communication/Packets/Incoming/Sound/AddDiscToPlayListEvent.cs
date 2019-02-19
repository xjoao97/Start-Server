#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Sound
{
    internal class AddDiscToPlayListEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            var room = Session.GetHabbo().CurrentRoom;
            if (!room.CheckRights(Session))
                return;
            //Console.WriteLine(Packet.ToString());

            var itemid = Packet.PopInt(); //item id
            // var songid = Packet.PopInt(); //Song id

            //var item = room.GetRoomItemHandler().GetItem(itemid);
            var item = Session.GetHabbo().GetInventoryComponent().GetItem(itemid);

            if (item == null)
                return;
            if (room.GetTraxManager().AddDisc(item, Session))
                return;
            Session.SendMessage(new RoomNotificationComposer("", "Oops! Não foi possível adicionar o disco!",
                "error", "", "", true));
        }
    }
}