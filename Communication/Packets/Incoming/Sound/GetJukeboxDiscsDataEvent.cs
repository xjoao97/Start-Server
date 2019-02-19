#region

using System.Collections.Generic;
using Oblivion.Communication.Packets.Outgoing.Sound;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms.TraxMachine;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Sound
{
    internal class GetJukeboxDiscsDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var songslen = Packet.PopInt();
            var Songs = new List<TraxMusicData>();
            while (songslen-- > 0)
            {
                var id = Packet.PopInt();
                var music = OblivionServer.GetGame().GetSoundManager().GetMusic(id);
                if (music != null)
                    Songs.Add(music);
            }
            if (Session.GetHabbo().CurrentRoom != null)
                Session.SendMessage(new SetJukeboxSongMusicDataComposer(Songs));
        }
    }
}