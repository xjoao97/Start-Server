#region

using System;
using System.Data;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.AI.Types
{
    internal class VisitorLogger : BotAI
    {
        private int VirtualId;

        public VisitorLogger(int VirtualId)
        {
            this.VirtualId = VirtualId;
        }

        public override void OnSelfEnterRoom()
        {
        }

        public override void OnSelfLeaveRoom(bool Kicked)
        {
        }

        public override void OnUserEnterRoom(RoomUser User)
        {
            if (GetBotData() == null)
                return;

            var Bot = GetRoomUser();

            if (User.GetClient().GetHabbo().CurrentRoom.OwnerId == User.GetClient().GetHabbo().Id)
            {
                DataTable getUsername;
                using (var query = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    query.SetQuery("SELECT username FROM room_visits WHERE roomid = @id");
                    query.AddParameter("id", User.RoomId);
                    getUsername = query.GetTable();
                }

                foreach (DataRow Row in getUsername.Rows)
                {
                    Bot.Chat(
                        "¡Me alegro de verlo Señor! Diga 'Si', si desea saber quien ha visitado la sala en su ausencia.",
                        false);
                    return;
                }
                Bot.Chat("He estado muy atento y te afirmo que nadie visitó esta sala mientras tú no estabas.", false);
            }
            else
            {
                Bot.Chat("Hola " + User.GetClient().GetHabbo().Username + ", le hablare de ti al dueño.", false);

                using (var query = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    query.SetQuery("INSERT INTO room_visits (roomid, username, gone) VALUE (@roomid, @username, @gone)");
                    query.AddParameter("roomid", User.RoomId);
                    query.AddParameter("username", User.GetClient().GetHabbo().Username);
                    query.AddParameter("gone", "todavía está en la sala.");
                    query.RunQuery();
                }
            }
        }


        public override void OnUserLeaveRoom(GameClient Client)
        {
            if (GetBotData() == null)
                return;

            var Bot = GetRoomUser();

            if (Client.GetHabbo().CurrentRoom.OwnerId == Client.GetHabbo().Id)
            {
                DataTable getRoom;

                using (var query = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    query.SetQuery("DELETE FROM room_visits WHERE roomid = @id");
                    query.AddParameter("id", Client.GetHabbo().CurrentRoom.RoomId);
                    getRoom = query.GetTable();
                }
            }
            DataTable getUpdate;

            using (var query = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                query.SetQuery("UPDATE room_visits SET gone = @gone WHERE roomid = @id AND username = @username");
                query.AddParameter("gone", "se ha ido");
                query.AddParameter("id", Client.GetHabbo().CurrentRoom.RoomId);
                query.AddParameter("username", Client.GetHabbo().Username);
                getUpdate = query.GetTable();
            }
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                return;

            if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, User.X, User.Y) > 8)
                return;

            switch (Message.ToLower())
            {
                case "si":
                case "yes":
                    if (GetBotData() == null)
                        return;

                    if (User.GetClient().GetHabbo().CurrentRoom.OwnerId == User.GetClient().GetHabbo().Id)
                    {
                        DataTable getRoomVisit;

                        using (var query = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            query.SetQuery("SELECT username, gone FROM room_visits WHERE roomid = @id");
                            query.AddParameter("id", User.RoomId);
                            getRoomVisit = query.GetTable();
                        }

                        foreach (DataRow Row in getRoomVisit.Rows)
                        {
                            var gone = Convert.ToString(Row["gone"]);
                            var username = Convert.ToString(Row["username"]);

                            GetRoomUser().Chat(username + " " + gone, false);
                        }
                        using (var query = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            query.SetQuery("DELETE FROM room_visits WHERE roomid = @id");
                            query.AddParameter("id", User.RoomId);
                            getRoomVisit = query.GetTable();
                        }
                    }
                    break;
            }
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
        }

        public override void OnTimerTick()
        {
        }
    }
}