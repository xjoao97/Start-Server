#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Trading
{
    internal class TradingAcceptEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;


            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.CanTradeInRoom)
                return;

            var Trade = Room.GetUserTrade(Session.GetHabbo().Id);

            Trade?.Accept(Session.GetHabbo().Id);
        }
    }
}