#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    internal class YouTubeVideoInformationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ItemId = Packet.PopInt();
            var VideoId = Packet.PopString();

            foreach (
                var Tele in
                OblivionServer.GetGame()
                    .GetTelevisionManager()
                    .TelevisionList.ToList()
                    .Where(Tele => Tele.YouTubeId == VideoId))
                Session.SendMessage(new GetYouTubeVideoComposer(ItemId, Tele.YouTubeId));
        }
    }
}