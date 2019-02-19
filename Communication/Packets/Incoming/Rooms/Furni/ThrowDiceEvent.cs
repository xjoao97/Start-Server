#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class ThrowDiceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;

            var Item = Room?.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Item == null)
                return;

            var hasRights = Room.CheckRights(Session, false, true);

            var request = Packet.PopInt();

            Item.Interactor.OnTrigger(Session, Item, request, hasRights);
        }
    }
}