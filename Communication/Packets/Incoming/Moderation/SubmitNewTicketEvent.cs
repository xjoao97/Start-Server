#region

using System.Collections.Generic;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms.Chat.Moderation;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class SubmitNewTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            if (
                OblivionServer.GetGame()
                    .GetModerationManager()
                    .GetModerationTool()
                    .UsersHasPendingTicket(Session.GetHabbo().Id))
            {
                Session.SendMessage(
                    new BroadcastMessageAlertComposer(
                        "You currently already have a pending ticket, please wait for a response from a moderator."));
                return;
            }

            var Message = Packet.PopString();
            var Type = Packet.PopInt();
            var ReportedUser = Packet.PopInt();
            var Room = Packet.PopInt();

            var Messagecount = Packet.PopInt();
            var Chats = new List<string>();
            for (var i = 0; i < Messagecount; i++)
            {
                Packet.PopInt();
                Chats.Add(Packet.PopString());
            }

            var Chat = new ModerationRoomChatLog(Packet.PopInt(), Chats);

            OblivionServer.GetGame()
                .GetModerationManager()
                .GetModerationTool()
                .SendNewTicket(Session, Type, ReportedUser, Message, Chats);
            OblivionServer.GetGame().GetClientManager().ModAlert("A new support ticket has been submitted!");
        }
    }
}