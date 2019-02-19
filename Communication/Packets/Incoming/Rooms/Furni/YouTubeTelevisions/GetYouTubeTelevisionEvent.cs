#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    internal class GetYouTubeTelevisionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var ItemId = Packet.PopInt();
            var Videos = OblivionServer.GetGame().GetTelevisionManager().TelevisionList;
            if (Videos.Count == 0)
            {
                Session.SendNotification(
                    "Oh, it looks like the hotel manager haven't added any videos for you to watch! :(");
                return;
            }

            var dict = OblivionServer.GetGame().GetTelevisionManager()._televisions;
            foreach (var value in RandomValues(dict).Take(1))
                Session.SendMessage(new GetYouTubeVideoComposer(ItemId, value.YouTubeId));

            Session.SendMessage(new GetYouTubePlaylistComposer(ItemId, Videos));
        }

        public IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            var rand = new Random();
            var values = dict.Values.ToList();
            var size = dict.Count;
            while (true)
                yield return values[rand.Next(size)];
        }
    }
}