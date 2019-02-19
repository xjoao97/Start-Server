using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Oblivion.HabboHotel.Support;
using Oblivion.HabboHotel.Rooms.Chat.Moderation;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.Moderation;

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    class ReportCameraPhotoEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (OblivionServer.GetGame().GetModerationManager().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id))
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("You currently already have a pending ticket, please wait for a response from a moderator."));
                return;
            }

            int photoId;

            if (!int.TryParse(Packet.PopString(), out photoId))
            {
                return;
            }

            int roomId = Packet.PopInt();
            int creatorId = Packet.PopInt();
            int categoryId = Packet.PopInt();

            OblivionServer.GetGame().GetModerationManager().GetModerationTool().SendNewTicket(Session, categoryId, creatorId, "", new List<string>(), 14, photoId);
            OblivionServer.GetGame().GetClientManager().ModAlert("A new support ticket has been submitted!");
        }
    }
}