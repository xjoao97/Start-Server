#region

using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Moderation
{
    public class ModerationTicket
    {
        public enum TicketStatus
        {
            OPEN = 0,
            PICKED = 1,
            RESOLVED = 2,
            ABUSIVE = 3,
            INVALID = 4,
            DELETED = 5
        }

        public ModerationTicket(int Id, int Type, int Category, double Timestamp, int Priority, Habbo Sender,
            Habbo Reported, string Issue, RoomData Room)
        {
            this.Id = Id;
            this.Type = Type;
            this.Category = Category;
            this.Timestamp = Timestamp;
            this.Priority = Priority;
            this.Sender = Sender;
            this.Reported = Reported;
            Moderator = null;
            this.Issue = Issue;
            this.Room = Room;
            Answered = false;
        }

        public int Id { get; set; }
        public int Type { get; set; }
        public int Category { get; set; }
        public double Timestamp { get; set; }
        public int Priority { get; set; }
        public bool Answered { get; set; }
        public Habbo Sender { get; set; }
        public Habbo Reported { get; set; }
        public Habbo Moderator { get; set; }
        public string Issue { get; set; }
        public RoomData Room { get; set; }

        public int GetStatus(int Id)
        {
            if (Moderator == null)
                return 1;
            if (Moderator.Id == Id && !Answered)
                return 2;
            return 3;
        }
    }
}