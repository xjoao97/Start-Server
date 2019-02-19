#region

using Oblivion.Communication.Packets.Outgoing.Sound;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Sound
{
    internal class LoadJukeboxDiscsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().CurrentRoom != null)
                Session.SendMessage(new LoadJukeboxUserMusicItemsComposer(Session));
        }
    }
}