﻿#region

using System;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class GroupInfoComposer : ServerPacket
    {
        public GroupInfoComposer(Group Group, GameClient Session, bool NewWindow = false)
            : base(ServerPacketHeader.GroupInfoMessageComposer)
        {
            var Origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Group.CreateTime);

            WriteInteger(Group.Id);
            WriteBoolean(true);
            WriteInteger(Group.GroupType == GroupType.OPEN ? 0 : Group.GroupType == GroupType.LOCKED ? 1 : 2);
            WriteString(Group.Name +
                        (Session.GetHabbo().GetPermissions().HasRight("mod_developer") ? "[" + Group.Id + "]" : ""));
            WriteString(Group.Description);
            WriteString(Group.Badge);
            WriteInteger(Group.RoomId);
            WriteString(OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId) == null
                ? "No room found.."
                : OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId).Name); // room name
            WriteInteger(Group.CreatorId == Session.GetHabbo().Id
                ? 3
                : Group.HasRequest(Session.GetHabbo().Id) ? 2 : Group.IsMember(Session.GetHabbo().Id) ? 1 : 0);
            WriteInteger(Group.MemberCount); // Members
            WriteBoolean(false); //?? CHANGED
            WriteString(Origin.Day + "-" + Origin.Month + "-" + Origin.Year);
            WriteBoolean(Group.CreatorId == Session.GetHabbo().Id);
            WriteBoolean(Group.IsAdmin(Session.GetHabbo().Id)); // admin
            WriteString(OblivionServer.GetUsernameById(Group.CreatorId));
            WriteBoolean(NewWindow); // Show group info
            WriteBoolean(Group.AdminOnlyDeco == 0); // Any user can place furni in home room
            WriteInteger(Group.CreatorId == Session.GetHabbo().Id
                ? Group.RequestCount
                : Group.IsAdmin(Session.GetHabbo().Id)
                    ? Group.RequestCount
                    : Group.IsMember(Session.GetHabbo().Id) ? 0 : 0); // Pending users
            //base.WriteInteger(0);//what the fuck
            WriteBoolean(Group != null ? Group.HasForum : true); //HabboTalk.
        }
    }
}