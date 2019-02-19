#region

using Oblivion.Communication.Packets.Outgoing.Talents;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Talents
{
    internal class GetTalentTrackEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Type = Packet.PopString();

//            var Levels = OblivionServer.GetGame().GetTalentTrackManager().GetLevels();

//            Session.SendMessage(new TalentTrackComposer(Levels, Type));
        }
    }
}