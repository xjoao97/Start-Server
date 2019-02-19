#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class UpdateGroupBadgeEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);

            if (Group?.CreatorId != Session.GetHabbo().Id)
                return;

            var Count = Packet.PopInt();
            var Current = 1;

            var newBadge = "";
            while (Current <= Count)
            {
                var Id = Packet.PopInt();
                var Colour = Packet.PopInt();
                var Pos = Packet.PopInt();
                string x;
                if (Current == 1)
                    x = "b" + (Id < 10 ? "0" + Id : Id.ToString()) + (Colour < 10 ? "0" + Colour : Colour.ToString()) +
                        Pos;
                else
                    x = "s" + (Id < 10 ? "0" + Id : Id.ToString()) + (Colour < 10 ? "0" + Colour : Colour.ToString()) +
                        Pos;
                newBadge += OblivionServer.GetGame().GetGroupManager().CheckActiveSymbol(x);
                Current++;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET badge = @badge WHERE id=" + Group.Id + " LIMIT 1");
                dbClient.AddParameter("badge", newBadge);
                dbClient.RunQuery();
            }

            Group.Badge = string.IsNullOrWhiteSpace(newBadge) ? "b05114s06114" : newBadge;
            Session.SendMessage(new GroupInfoComposer(Group, Session));
        }
    }
}