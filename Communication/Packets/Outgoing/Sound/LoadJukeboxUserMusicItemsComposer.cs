#region

using System.Collections.Generic;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Sound
{
    internal class LoadJukeboxUserMusicItemsComposer : ServerPacket
    {
        public LoadJukeboxUserMusicItemsComposer(GameClient Session)
            : base(ServerPacketHeader.LoadJukeboxUserMusicItemsMessageComposer)
        {
            var room = Session.GetHabbo().CurrentRoom;
            var songs = room.GetTraxManager().GetUserSongs(Session);

            WriteInteger(songs.Count); //while

            foreach (var item in songs)
            {
                WriteInteger(item.Id); //item id
                WriteInteger(room.GetTraxManager().GetExtraData(item)); //Song id
            }
        }

        public LoadJukeboxUserMusicItemsComposer(ICollection<Item> Items)
            : base(ServerPacketHeader.LoadJukeboxUserMusicItemsMessageComposer)
        {
            WriteInteger(Items.Count); //while

            foreach (var item in Items)
            {
                WriteInteger(item.Id); //item id
                WriteInteger(item.GetRoom().GetTraxManager().GetExtraData(item)); //Song id
            }
        }
    }
}