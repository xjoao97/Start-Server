#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.Cache;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class GetGroupMembersEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var Page = Packet.PopInt();
            var SearchVal = Packet.PopString();
            var RequestType = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return;

            var Members = new List<UserCache>();

            switch (RequestType)
            {
                case 0:
                {
                    var MemberIds = Group.GetAllMembers;
                    foreach (
                        var GroupMember in
                        MemberIds.ToList()
                            .Select(Id => OblivionServer.GetGame().GetCacheManager().GenerateUser(Id))
                            .Where(GroupMember => GroupMember != null)
                            .Where(GroupMember => !Members.Contains(GroupMember)))
                        Members.Add(GroupMember);
                    break;
                }

                case 1:
                {
                    var AdminIds = Group.GetAdministrators;
                    foreach (
                        var GroupMember in
                        AdminIds.ToList()
                            .Select(Id => OblivionServer.GetGame().GetCacheManager().GenerateUser(Id))
                            .Where(GroupMember => GroupMember != null)
                            .Where(GroupMember => !Members.Contains(GroupMember)))
                        Members.Add(GroupMember);
                    break;
                }

                case 2:
                {
                    var RequestIds = Group.GetRequests;
                    foreach (
                        var GroupMember in
                        RequestIds.ToList()
                            .Select(Id => OblivionServer.GetGame().GetCacheManager().GenerateUser(Id))
                            .Where(GroupMember => GroupMember != null)
                            .Where(GroupMember => !Members.Contains(GroupMember)))
                        Members.Add(GroupMember);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(SearchVal))
                Members = Members.Where(x => x.Username.ToLower().Contains(SearchVal.ToLower())).ToList();

            var StartIndex = (Page - 1) * 14 + 14;
            var FinishIndex = Members.Count;

            Session.SendMessage(new GroupMembersComposer(Group,
                Members.Skip(StartIndex).Take(FinishIndex - StartIndex).ToList(), Members.Count, Page,
                Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id), RequestType,
                SearchVal));
        }
    }
}