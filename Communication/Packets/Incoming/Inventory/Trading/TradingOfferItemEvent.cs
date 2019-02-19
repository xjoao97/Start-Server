#region

using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Trading
{
    internal class TradingOfferItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CanTradeInRoom)
                return;

            var Trade = Room.GetUserTrade(Session.GetHabbo().Id);
            if (Trade == null)
                return;

            Task.Factory.StartNew(() =>
            {
                var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Packet.PopInt());
                if (Item == null)
                    return;

                Trade.OfferItem(Session.GetHabbo().Id, Item);
            });
        }
    }
}