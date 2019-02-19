#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class UpdateGroupColoursEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var Colour1 = Packet.PopInt();
            var Colour2 = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);

            if (Group?.CreatorId != Session.GetHabbo().Id)
                return;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` =' " +
                                  Group.Id + "' LIMIT 1");
                dbClient.AddParameter("colour1", Colour1);
                dbClient.AddParameter("colour2", Colour2);
                dbClient.RunQuery();
            }

            Group.Colour1 = Colour1;
            Group.Colour2 = Colour2;

            Session.SendMessage(new GroupInfoComposer(Group, Session));
            if (Session.GetHabbo().CurrentRoom == null) return;
            foreach (
                var Item in
                Session.GetHabbo()
                    .CurrentRoom.GetRoomItemHandler()
                    .GetFloor.ToList()
                    .Where(Item => Item?.GetBaseItem() != null)
                    .Where(
                        Item =>
                            (Item.GetBaseItem().InteractionType == InteractionType.GuildItem ||
                             Item.GetBaseItem().InteractionType == InteractionType.GuildGate) &&
                            Item.GetBaseItem().InteractionType == InteractionType.GuildForum))
                Session.GetHabbo()
                    .CurrentRoom.SendMessage(new ObjectUpdateComposer(Item, Convert.ToInt32(Item.UserID)));
        }
    }
}