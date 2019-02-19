#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Televisions;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions
{
    internal class GetYouTubePlaylistComposer : ServerPacket
    {
        public GetYouTubePlaylistComposer(int ItemId, ICollection<TelevisionItem> Videos)
            : base(ServerPacketHeader.GetYouTubePlaylistMessageComposer)
        {
            WriteInteger(ItemId);
            WriteInteger(Videos.Count);
            foreach (var Video in Videos.ToList())
            {
                WriteString(Video.YouTubeId);
                WriteString(Video.Title); //Title
                WriteString(Video.Description); //Description
            }
            WriteString("");
        }
    }
}