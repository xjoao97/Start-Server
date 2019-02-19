#region

using System.Collections.Generic;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Moderation
{
    internal class ModerationRoomChatLog
    {
        public ModerationRoomChatLog(int UserId, List<string> Chat)
        {
            this.UserId = UserId;
            this.Chat = Chat;
        }

        public int UserId { get; set; }
        public List<string> Chat { get; set; }
    }
}