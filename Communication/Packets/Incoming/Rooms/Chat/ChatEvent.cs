#region

using System;
using System.Text;
using System.Web;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Moderation;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms.Chat.Styles;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Chat
{
    public class ChatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;
            string Message;
            if (OblivionServer.GetConfig().data["chat.html"] == "0")
            {
                 Message =
                    HttpUtility.HtmlEncode(
                        Encoding.UTF8.GetString(
                            Encoding.Default.GetBytes(StringCharFilter.Escape(Packet.PopString(), false, false))));
            }
            else
            {
                 Message = StringCharFilter.Escape(Packet.PopString(), false, false);
            }
            if (Message.Length > 100)
                Message = Message.Substring(0, 100);

            if (Session.GetHabbo().Rank < 8 &&
                           OblivionServer.GetGame().GetChatManager().GetFilter().IsFiltered(Message, true))
            {
                OblivionServer.GetGame()
                    .GetClientManager()
                    .StaffAlert(new RoomNotificationComposer("",
                        "O usuário: " + Session.GetHabbo().Username + " divulgou o seguinte Hotel: " + Message +
                        "\nPara ir na sala, clique aqui.",
                        "divulgando", "Ir para a sala", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId, true));
                Session.SendWhisper("A palavra:" + " " + Message + " está proibida no Hotel.");
                return;
            }
            
            var Colour = Packet.PopInt();

            ChatStyle Style;
            if (!OblivionServer.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) ||
                Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight))
                Colour = 0;

            User.UnIdle();

            if (OblivionServer.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime > 0)
                return;

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendMessage(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Opa, você foi mutado.");
                return;
            }

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

            if (Message.StartsWith(":", StringComparison.CurrentCulture) &&
                OblivionServer.GetGame().GetChatManager().GetCommands().Parse(Session, Message))
                return;


//            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
//                Message = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Message);


            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_CHAT);

            User.OnChat(User.LastBubble, Message, false);
        }
    }
}