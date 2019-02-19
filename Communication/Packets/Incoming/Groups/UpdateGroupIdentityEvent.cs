#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class UpdateGroupIdentityEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var Name = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Desc = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);

            if (Group?.CreatorId != Session.GetHabbo().Id)
                return;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `name`= @name, `desc` = @desc WHERE `id` = '" + GroupId +
                                  "' LIMIT 1");
                dbClient.AddParameter("name", Name);
                dbClient.AddParameter("desc", Desc);
                dbClient.RunQuery();
            }

            Group.Name = Name;
            Group.Description = Desc;

            Session.SendMessage(new GroupInfoComposer(Group, Session));
        }
    }
}