#region

using Oblivion.Communication.Packets.Outgoing.Inventory.Bots;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Bots
{
    internal class GetBotInventoryEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
                return;

            var Bots = Session.GetHabbo().GetInventoryComponent().GetBots();
            Session.SendMessage(new BotInventoryComposer(Bots));
        }
    }
}