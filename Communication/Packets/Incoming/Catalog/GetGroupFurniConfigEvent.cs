#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class GetGroupFurniConfigEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(
            new GroupFurniConfigComposer(
                OblivionServer.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id)));
    }
}