#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.AI
{
    public abstract class BotAI
    {
        public int BaseId;
        private Room room;
        private int RoomId;
        private RoomUser roomUser;
        private int RoomUserId;

        public void Init(int pBaseId, int pRoomUserId, int pRoomId, RoomUser user, Room room)
        {
            BaseId = pBaseId;
            RoomUserId = pRoomUserId;
            RoomId = pRoomId;
            roomUser = user;
            this.room = room;
        }

        public Room GetRoom() => room;

        public RoomUser GetRoomUser() => roomUser;

        public RoomBot GetBotData()
        {
            var User = GetRoomUser();
            return User == null ? null : GetRoomUser().BotData;
        }

        public abstract void OnSelfEnterRoom();
        public abstract void OnSelfLeaveRoom(bool Kicked);
        public abstract void OnUserEnterRoom(RoomUser User);
        public abstract void OnUserLeaveRoom(GameClient Client);
        public abstract void OnUserSay(RoomUser User, string Message);
        public abstract void OnUserShout(RoomUser User, string Message);
        public abstract void OnTimerTick();
    }
}