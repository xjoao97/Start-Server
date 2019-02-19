#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Quests
{
    internal class StartQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var QuestId = Packet.PopInt();

            OblivionServer.GetGame().GetQuestManager().ActivateQuest(Session, QuestId);
        }
    }
}