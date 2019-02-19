#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Moderation;

#endregion

namespace Oblivion.HabboHotel.Support
{
    /// <summary>
    ///     TODO: Utilize ModerationTicket.cs
    /// </summary>
    public class ModerationTool
    {
        public List<SupportTicket> Tickets;

        public ModerationTool()
        {
            Tickets = new List<SupportTicket>();
        }

        public ICollection<SupportTicket> GetTickets => Tickets;

        #region Support Tickets

        public void SendNewTicket(GameClient Session, int Category, int ReportedUser, string Message,
            List<string> Messages, int Type = 1, int ExtraId = 0)
        {
            int TicketId;
            SupportTicket Ticket;

            if (Session.GetHabbo().CurrentRoomId <= 0)
            {
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'" +
                        Category + "', 'open', '" + Session.GetHabbo().Id + "', '" + ReportedUser +
                        "', '0', @message, '0', '', '" + OblivionServer.GetUnixTimestamp() + "')");
                    dbClient.AddParameter("message", Message);
                    TicketId = Convert.ToInt32(dbClient.InsertQuery());

                    dbClient.RunQuery("UPDATE `user_info` SET `cfhs` = `cfhs` + '1' WHERE `user_id` = '" +
                                      Session.GetHabbo().Id + "' LIMIT 1");
                }

                Ticket = new SupportTicket(TicketId, 1, 7, Category, Session.GetHabbo().Id, ReportedUser, Message, 0, "",
                    OblivionServer.GetUnixTimestamp(), Messages);

                Tickets.Add(Ticket);

                SendTicketToModerators(Ticket);
                return;
            }

            var Data = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Session.GetHabbo().CurrentRoomId);

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1, '" +
                    Category + "', 'open', '" + Session.GetHabbo().Id + "', '" + ReportedUser + "', '0', @message, '" +
                    Data.Id + "', @name,'" + OblivionServer.GetUnixTimestamp() + "')");
                dbClient.AddParameter("message", Message);
                dbClient.AddParameter("name", Data.Name);
                TicketId = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.RunQuery("UPDATE user_info SET cfhs = cfhs + 1 WHERE user_id = '" + Session.GetHabbo().Id +
                                  "' LIMIT 1");
            }

            Ticket = new SupportTicket(TicketId, 1, 7, Category, Session.GetHabbo().Id, ReportedUser, Message, Data.Id,
                Data.Name, OblivionServer.GetUnixTimestamp(), Messages);
            Tickets.Add(Ticket);
            SendTicketToModerators(Ticket);
        }

        public SupportTicket GetTicket(int TicketId) => Tickets.FirstOrDefault(Ticket => Ticket.TicketId == TicketId);

        public void PickTicket(GameClient Session, int TicketId)
        {
            var Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != ModerationTicket.TicketStatus.OPEN)
                return;

            Ticket.Pick(Session.GetHabbo().Id, true);
            SendTicketToModerators(Ticket);
        }

        public void ReleaseTicket(GameClient Session, int TicketId)
        {
            var Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != ModerationTicket.TicketStatus.PICKED ||
                Ticket.ModeratorId != Session.GetHabbo().Id)
                return;

            Ticket.Release();
            SendTicketToModerators(Ticket);
        }

        public void CloseTicket(GameClient Session, int TicketId, int Result)
        {
            var Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != ModerationTicket.TicketStatus.PICKED ||
                Ticket.ModeratorId != Session.GetHabbo().Id)
                return;

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Ticket.SenderId);


            ModerationTicket.TicketStatus NewStatus;
            var ResultCode = 0;
            switch (Result)
            {
                case 1:
                {
                    ResultCode = 1;
                    NewStatus = ModerationTicket.TicketStatus.INVALID;
                }
                    break;

                case 2:
                {
                    ResultCode = 2;
                    NewStatus = ModerationTicket.TicketStatus.ABUSIVE;

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery(
                            "UPDATE `user_info` SET `cfhs_abusive` = `cfhs_abusive` + 1 WHERE `user_id` = '" +
                            Ticket.SenderId + "' LIMIT 1");
                    }
                }
                    break;

                case 3:
                default:
                {
                    ResultCode = 0;
                    NewStatus = ModerationTicket.TicketStatus.RESOLVED;
                }
                    break;
            }

            Client?.SendMessage(new ModeratorSupportTicketResponseComposer(ResultCode));

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery(
                    "UPDATE `user_stats` SET `tickets_answered` = `tickets_answered` + '1' WHERE `id` = '" +
                    Session.GetHabbo().Id + "' LIMIT 1");
            }
            Ticket.Close(NewStatus);
            SendTicketToModerators(Ticket);
        }

        public bool UsersHasPendingTicket(int Id)
            => Tickets.Any(Ticket => Ticket.SenderId == Id && Ticket.Status == ModerationTicket.TicketStatus.OPEN);


        public static void SendTicketToModerators(SupportTicket Ticket)
            =>
                OblivionServer.GetGame()
                    .GetClientManager()
                    .SendMessage(new ModeratorSupportTicketComposer(Ticket), "mod_tool");

        #endregion
    }
}