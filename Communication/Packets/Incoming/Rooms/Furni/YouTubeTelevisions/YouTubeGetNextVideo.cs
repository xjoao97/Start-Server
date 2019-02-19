#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Televisions;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    internal class YouTubeGetNextVideo : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Videos = OblivionServer.GetGame().GetTelevisionManager().TelevisionList;

            if (Videos.Count == 0)
            {
                Session.SendNotification(
                    "Oh, it looks like the hotel manager haven't added any videos for you to watch! :(");
                return;
            }

            var ItemId = Packet.PopInt();
            var Next = Packet.PopInt();

            TelevisionItem Item = null;
            var dict = OblivionServer.GetGame().GetTelevisionManager()._televisions;
            foreach (var value in RandomValues(dict).Take(1))
                Item = value;

            if (Item == null)
            {
                Session.SendNotification("Oh, it looks like their was a problem getting the video.");
                return;
            }

            Session.SendMessage(new GetYouTubeVideoComposer(ItemId, Item.YouTubeId));
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