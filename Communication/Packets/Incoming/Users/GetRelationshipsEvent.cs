#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class GetRelationshipsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Habbo = OblivionServer.GetHabboById(Packet.PopInt());
            if (Habbo == null)
                return;

            var rand = new Random();
            Habbo.Relationships = Habbo.Relationships.OrderBy(x => rand.Next())
                .ToDictionary(item => item.Key, item => item.Value);

            var Loves = Habbo.Relationships.Count(x => x.Value.Type == 1);
            var Likes = Habbo.Relationships.Count(x => x.Value.Type == 2);
            var Hates = Habbo.Relationships.Count(x => x.Value.Type == 3);

            Session.SendMessage(new GetRelationshipsComposer(Habbo, Loves, Likes, Hates));
        }
    }
}