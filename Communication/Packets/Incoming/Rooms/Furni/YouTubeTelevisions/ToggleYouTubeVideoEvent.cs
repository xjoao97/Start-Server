#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class ToggleYouTubeVideoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ItemId = Packet.PopInt(); //Item Id
            var VideoId = Packet.PopString(); //Video ID

            Session.SendMessage(new GetYouTubeVideoComposer(ItemId, VideoId));
        }
    }
}