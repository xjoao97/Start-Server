﻿#region

using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class GroupFurniSettingsComposer : ServerPacket
    {
        public GroupFurniSettingsComposer(Group Group, int ItemId, int UserId)
            : base(ServerPacketHeader.GroupFurniSettingsMessageComposer)
        {
            WriteInteger(ItemId); //Item Id
            WriteInteger(Group.Id); //Group Id?
            WriteString(Group.Name);
            WriteInteger(Group.RoomId); //RoomId
            WriteBoolean(Group.IsMember(UserId)); //Member?
            WriteBoolean(Group.ForumEnabled); //Has a forum
        }
    }
}