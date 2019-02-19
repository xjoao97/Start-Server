#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Moderation;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms.Chat.Styles;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Chat
{
    public class WhisperEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;


            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Opa, você está mudo.");
                return;
            }

            if (OblivionServer.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime > 0)
                return;


            var Params = Packet.PopString();
            var ToUser = Params.Split(' ')[0];
            var Message = Params.Substring(ToUser.Length + 1);

            if (OblivionServer.GetGame().GetChatManager().GetFilter().IsFiltered(Message, true))
            {
                OblivionServer.GetGame()
                    .GetClientManager()
                    .StaffAlert(new RoomNotificationComposer("Alerta de Divulgação",
                        "O usuário: <b>" + Session.GetHabbo().Username + "<br>" +
                        "<br></b> Está divulgando algum Hotel, exerça sua função como Staff e vá bani-lo.<br>Antes de tomar esta decisão, leia o Chatlog." +
                        "<br>" +
                        "<br><b>A palavra usada pelo meliante foi:</b><br>" +
                        "<br>" + "<b>" + "<font color =\"#FF0000\">" + Message + "</font>" + "</b><br>" +
                        "<br>Para ir na sala, clique em \"Ir para a Sala \"</b>",
                        "filter", "Ir para a sala", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                Session.SendWhisper("A palavra:" + " " + Message + " está proibida no Hotel.");
                return;
            }

            var Colour = Packet.PopInt();

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            var User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(ToUser);
            if (User2 == null)
                return;

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendMessage(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }


            ChatStyle Style;
            if (!OblivionServer.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) ||
                Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight))
                Colour = 0;

            User.LastBubble = Session.GetHabbo().CustomBubbleId == 0 ? Colour : Session.GetHabbo().CustomBubbleId;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                int MuteTime;
                if (User.IncrementAndCheckFlood(out MuteTime))
                {
                    Session.SendMessage(new FloodControlComposer(MuteTime));
                    return;
                }
            }

            if (!User2.GetClient().GetHabbo().ReceiveWhispers &&
                !Session.GetHabbo().GetPermissions().HasRight("room_whisper_override"))
            {
                Session.SendWhisper("Oops, this user has their whispers disabled!");
                return;
            }

//            OblivionServer.GetGame()
//                .GetChatManager()
//                .GetLogs()
//                .StoreChatlog(new ChatlogEntry(Session.GetHabbo().Id, Room.Id, "<Whisper to " + ToUser + ">: " + Message,
//                    UnixTimestamp.GetNow()));

//            Room.AddChatlog(Session.GetHabbo().Id, "<Susurra a " + ToUser + ">: " + Message);



            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_CHAT);

            User.UnIdle();
            User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));

            if (!User2.IsBot && User2.UserId != User.UserId)
                if (!User2.GetClient().GetHabbo().MutedUsers.Contains(Session.GetHabbo().Id))
                    User2.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));

            var ToNotify = Room.GetRoomUserManager().GetRoomUserByRank(2);
            if (ToNotify.Count <= 0) return;
            foreach (
                var user in
                ToNotify.Where(user => user != null && user.HabboId != User2.HabboId && user.HabboId != User.HabboId)
                    .Where(user => user.GetClient() != null && user.GetClient().GetHabbo() != null &&
                                   !user.GetClient().GetHabbo().IgnorePublicWhispers))
                user.GetClient()
                    .SendMessage(new WhisperComposer(User.VirtualId, "[Susurra a " + ToUser + "] " + Message,
                        0, User.LastBubble));
        }
    }
}