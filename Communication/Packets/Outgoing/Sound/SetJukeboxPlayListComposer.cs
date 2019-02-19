#region

using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Sound
{
    internal class SetJukeboxPlayListComposer : ServerPacket
    {
        public SetJukeboxPlayListComposer(Room room)
            : base(ServerPacketHeader.SetJukeboxPlayListMessageComposer)
        {
            var items = room.GetTraxManager().Playlist;
            WriteInteger(items.Count); //Capacity
            WriteInteger(items.Count); //While items Songs Count

            foreach (var item in items)
            {
                var music = room.GetTraxManager().GetExtraData(item);
                WriteInteger(item.Id);
                WriteInteger(music); //EndWhile
            }
        }
    }
}