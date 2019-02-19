#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class DiceOffEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;

            var Item = Room?.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Item == null)
                return;

            var hasRights = Room.CheckRights(Session);

            Item.Interactor.OnTrigger(Session, Item, -1, hasRights);
        }
    }
}