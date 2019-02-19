#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class GetBadgeEditorPartsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(new BadgeEditorPartsComposer(
            OblivionServer.GetGame().GetGroupManager().Bases,
            OblivionServer.GetGame().GetGroupManager().Symbols,
            OblivionServer.GetGame().GetGroupManager().BaseColours,
            OblivionServer.GetGame().GetGroupManager().SymbolColours,
            OblivionServer.GetGame().GetGroupManager().BackGroundColours));
    }
}