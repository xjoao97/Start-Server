#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetFurnitureAliasesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => Session.SendMessage(new FurnitureAliasesComposer());
    }
}