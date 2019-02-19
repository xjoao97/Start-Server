#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Purse
{
    internal class GetHabboClubWindowEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => Session.SendNotification("La suscripción en el HC, es gratuita para todos los miembros");
    }
}