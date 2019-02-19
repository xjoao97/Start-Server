#region

using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Trading
{
    internal class TradingOfferItemsEvent : IPacketEvent
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


          /*  if (Trade.CountItems(Trade.GetTradeUser(Session.GetHabbo().Id)) > 50)
            {
                Session.SendNotification("Máximo de 50 ítens por troca.");
                return;
            }*/
            var Amount = Packet.PopInt();
//            if (Amount > 50)
//                Amount = 50;

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Packet.PopInt());
            if (Item == null)
                return;
            Task.Factory.StartNew(() =>
            {
                var AllItems =
                    Session.GetHabbo()
                        .GetInventoryComponent()
                        .GetItems.Where(x => x.Data.Id == Item.Data.Id)
                        .Take(Amount)
                        .ToList();
                foreach (var I in AllItems)
                {
                    Trade.OfferItem(Session.GetHabbo().Id, I);
                }
            });
        }
    }
}