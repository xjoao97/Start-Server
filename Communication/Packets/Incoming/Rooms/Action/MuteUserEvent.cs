#region

using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class MuteUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;
            //neste codigo podemos ver 1 gambiarra pra agradar japas!
            var UserId = Packet.PopInt();
            var RoomId = Packet.PopInt();
            var Time = Packet.PopInt();
            var blockedcommands = Session.GetHabbo().CurrentRoom.RoomData.BlockedCommands;
            switch (Time)
            {
                case 60:
                    if (blockedcommands.Contains("sex") && Session.GetHabbo().Rank < 5)
                    {
                        Session.SendWhisper(Language.GetValue("command.blocked"));
                        return;
                    }
                    var Sex = new SexCommand();
                    Sex.Command(Session, OblivionServer.GetUsernameById(UserId));
                    return;
                case 1080:
                    if (blockedcommands.Contains("kill") && Session.GetHabbo().Rank < 5)
                    {
                        Session.SendWhisper(Language.GetValue("command.blocked"));
                        return;
                    }
                    var Kill = new KillCommand();
                    Kill.Command(Session, OblivionServer.GetUsernameById(UserId));
                    return;
            }
            //if are normal mute xD
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (Room.WhoCanMute == 0 && !Room.CheckRights(Session, true) && Room.Group == null ||
                Room.WhoCanMute == 1 && !Room.CheckRights(Session) && Room.Group == null ||
                Room.Group != null && !Room.CheckRights(Session, false, true))
                return;

            var Target = Room.GetRoomUserManager().GetRoomUserByHabbo(OblivionServer.GetUsernameById(UserId));

            if (Target == null || Target.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            if (Room.MutedUsers.ContainsKey(UserId))
                if (Room.MutedUsers[UserId] < OblivionServer.GetUnixTimestamp())
                    Room.MutedUsers.Remove(UserId);
                else
                    return;

            Room.MutedUsers.Add(UserId, OblivionServer.GetUnixTimestamp() + Time * 60);

            Target.GetClient().SendWhisper("The room owner has muted you for " + Time + " minutes!");
            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModMuteSeen", 1);
        }
    }
}