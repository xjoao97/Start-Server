#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Moderation;

#endregion

namespace Oblivion.HabboHotel.Support
{
    /// <summary>
    ///     TODO: Utilize ModerationTicket.cs
    /// </summary>
    public class SupportTicket
    {
        public readonly int Id;
        public readonly string ReportedName;
        public readonly string SenderName;
        public int Category;

        public string Message;
        public int ModeratorId;
        public string ModName;
        public List<string> ReportedChats;
        public int ReportedId;

        public int RoomId;
        public string RoomName;
        public int Score;
        public int SenderId;
        public ModerationTicket.TicketStatus Status;
        public double Timestamp;

        public int Type;


        public SupportTicket(int Id, int Score, int Type, int Category, int SenderId, int ReportedId, string Message,
            int RoomId, string RoomName, double Timestamp, List<string> ReportedChats)
        {
            this.Id = Id;
            this.Score = Score;
            this.Type = Type;
            this.Category = Category;
            Status = ModerationTicket.TicketStatus.OPEN;
            this.SenderId = SenderId;
            this.ReportedId = ReportedId;
            ModeratorId = 0;
            this.Message = Message;
            this.RoomId = RoomId;
            this.RoomName = RoomName;
            this.Timestamp = Timestamp;
            this.ReportedChats = ReportedChats;


            SenderName = OblivionServer.GetGame().GetClientManager().GetNameById(SenderId);
            ReportedName = OblivionServer.GetGame().GetClientManager().GetNameById(ReportedId);
            ModName = OblivionServer.GetGame().GetClientManager().GetNameById(ModeratorId);
        }

        public int TabId
        {
            get
            {
                if (Status == ModerationTicket.TicketStatus.OPEN)
                    return 1;

                if (Status == ModerationTicket.TicketStatus.PICKED)
                    return 2;

                if (Status == ModerationTicket.TicketStatus.ABUSIVE || Status == ModerationTicket.TicketStatus.INVALID ||
                    Status == ModerationTicket.TicketStatus.RESOLVED)
                    return 0;

                if (Status == ModerationTicket.TicketStatus.DELETED)
                    return 0;

                return 0;
            }
        }

        public int TicketId => Id;

        public void Pick(int pModeratorId, bool UpdateInDb)
        {
            Status = ModerationTicket.TicketStatus.PICKED;
            ModeratorId = pModeratorId;
            ModName = OblivionServer.GetHabboById(pModeratorId).Username;
            if (UpdateInDb)
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE moderation_tickets SET status = 'picked', moderator_id = " + pModeratorId +
                                      ", timestamp = '" + OblivionServer.GetUnixTimestamp() + "' WHERE id = " + Id + "");
                }
        }

        public void Close(ModerationTicket.TicketStatus NewStatus)
        {
            Status = NewStatus;

            var dbType = "";

            switch (NewStatus)
            {
                case ModerationTicket.TicketStatus.ABUSIVE:

                    dbType = "abusive";
                    break;

                case ModerationTicket.TicketStatus.INVALID:

                    dbType = "invalid";
                    break;

                case ModerationTicket.TicketStatus.RESOLVED:
                default:

                    dbType = "resolved";
                    break;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE moderation_tickets SET status = '" + dbType + "' WHERE id = " + Id +
                                  " LIMIT 1");
            }
        }

        public void Release()
        {
            Status = ModerationTicket.TicketStatus.OPEN;


            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE moderation_tickets SET status = 'open' WHERE id = " + Id + " LIMIT 1");
            }
        }
    }
}