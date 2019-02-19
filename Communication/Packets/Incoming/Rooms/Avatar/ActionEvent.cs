#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Avatar
{
    public class ActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Action = Packet.PopInt();

            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (User.DanceId > 0)
                User.DanceId = 0;

            if (Session.GetHabbo().Effects().CurrentEffect > 0)
                Room.SendMessage(new AvatarEffectComposer(User.VirtualId, 0));

            User.UnIdle();
            Room.SendMessage(new ActionComposer(User.VirtualId, Action));

            if (Action == 5) // idle
            {
                User.IsAsleep = true;
                Room.SendMessage(new SleepComposer(User, true));
            }

            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_WAVE);
        }
    }
}