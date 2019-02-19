#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Action;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class IgnoreUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var Username = Packet.PopString();
            var User = OblivionServer.GetHabboByUsername(Username);
            if (User == null || Session.GetHabbo().MutedUsers.Contains(User.Id) ||
                User.GetPermissions().HasRight("mod_tool"))
                return;

            Session.GetHabbo().MutedUsers.Add(User.Id);
            Session.SendMessage(new IgnoreStatusComposer(1, Username));

            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModIgnoreSeen", 1);
        }
    }
}