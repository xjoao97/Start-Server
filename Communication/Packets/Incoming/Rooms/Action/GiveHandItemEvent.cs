#region

using System;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class GiveHandItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            var TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Packet.PopInt());
            if (TargetUser == null)
                return;

            if (!(Math.Abs(User.X - TargetUser.X) >= 3 || Math.Abs(User.Y - TargetUser.Y) >= 3) ||
                Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                if (User.CarryItemID > 0 && User.CarryTimer > 0)
                {
                    if (User.CarryItemID == 8)
                        OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.GIVE_COFFEE);
                    TargetUser.CarryItem(User.CarryItemID);
                    User.CarryItem(0);
                    TargetUser.DanceId = 0;
                }
        }
    }
}