#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Quests
{
    public class GetQuestListEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => OblivionServer.GetGame().GetQuestManager().GetList(Session, null);
    }
}