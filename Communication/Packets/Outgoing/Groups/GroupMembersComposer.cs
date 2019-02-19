#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Cache;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class GroupMembersComposer : ServerPacket
    {
        public GroupMembersComposer(Group Group, ICollection<UserCache> Members, int MembersCount, int Page, bool Admin,
            int ReqType, string SearchVal)
            : base(ServerPacketHeader.GroupMembersMessageComposer)
        {
            WriteInteger(Group.Id);
            WriteString(Group.Name);
            WriteInteger(Group.RoomId);
            WriteString(Group.Badge);
            WriteInteger(MembersCount);

            WriteInteger(Members.Count);
            if (MembersCount > 0)
                foreach (var Data in Members)
                {
                    WriteInteger(Group.CreatorId == Data.Id
                        ? 0
                        : Group.IsAdmin(Data.Id) ? 1 : Group.IsMember(Data.Id) ? 2 : 3);
                    WriteInteger(Data.Id);
                    WriteString(Data.Username);
                    WriteString(Data.Look);
                    WriteString(string.Empty);
                }
            WriteBoolean(Admin);
            WriteInteger(14);
            WriteInteger(Page);
            WriteInteger(ReqType);
            WriteString(SearchVal);
        }
    }
}