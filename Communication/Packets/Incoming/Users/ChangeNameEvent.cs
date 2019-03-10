#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class ChangeNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (User == null)
                return;

            var NewName = Packet.PopString();
            var OldName = Session.GetHabbo().Username;

            if (NewName == OldName)
            {
                Session.GetHabbo().ChangeName(OldName);
                Session.SendMessage(new UpdateUsernameComposer(NewName));
                return;
            }

            if (!CanChangeName(Session.GetHabbo()))
            {
                Session.SendNotification("Oops, it appears you currently cannot change your username!");
                return;
            }

            bool InUse;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", NewName);
                InUse = dbClient.GetInteger() == 1;
            }

            var Letters = NewName.ToLower().ToCharArray();
            const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";

            if (Letters.Any(Chr => !AllowedCharacters.Contains(Chr)))
                return;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && NewName.ToLower().Contains("mod") ||
                NewName.ToLower().Contains("adm") || NewName.ToLower().Contains("admin")
                || NewName.ToLower().Contains("m0d") || NewName.ToLower().Contains("mob") ||
                NewName.ToLower().Contains("m0b"))
                return;
            if (!NewName.ToLower().Contains("mod") && (Session.GetHabbo().Rank == 2 || Session.GetHabbo().Rank == 3))
                return;
            if (NewName.Length > 15)
                return;
            if (NewName.Length < 3)
                return;
            if (InUse)
                return;
            if (!OblivionServer.GetGame().GetClientManager().UpdateClientUsername(Session, OldName, NewName))
            {
                Session.SendNotification("Oops! An issue occoured whilst updating your username.");
                return;
            }

            Session.GetHabbo().ChangingName = false;

            Room.GetRoomUserManager().RemoveUserFromRoom(Session, true);

            Session.GetHabbo().ChangeName(NewName);
            Session.GetHabbo().GetMessenger().OnStatusChanged(true);

            Session.SendMessage(new UpdateUsernameComposer(NewName));
            Room.SendMessage(new UserNameChangeComposer(Room.Id, User.VirtualId, NewName));

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `logs_client_namechange` (`user_id`,`new_name`,`old_name`,`timestamp`) VALUES ('" +
                    Session.GetHabbo().Id + "', @name, '" + OldName + "', '" + OblivionServer.GetUnixTimestamp() + "')");
                dbClient.AddParameter("name", NewName);
                dbClient.RunQuery();
            }

            ICollection<RoomData> Rooms = Session.GetHabbo().UsersRooms;
            foreach (var Data in Rooms.Where(Data => Data != null))
                Data.OwnerName = NewName;

            foreach (
                var UserRoom in
                OblivionServer.GetGame()
                    .GetRoomManager()
                    .GetRooms()
                    .ToList()
                    .Where(UserRoom => UserRoom != null && UserRoom.RoomData.OwnerName == NewName))
            {
                UserRoom.OwnerName = NewName;
                UserRoom.RoomData.OwnerName = NewName;

                UserRoom.SendMessage(new RoomInfoUpdatedComposer(UserRoom.RoomId));
            }

            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Name", 1);

            Session.SendMessage(new RoomForwardComposer(Room.Id));
        }

        private static bool CanChangeName(Habbo Habbo)
        {
            if (Habbo.Rank == 1 && Habbo.Rank == 0 && Habbo.LastNameChange == 0)
                return true;
            if (Habbo.Rank == 1 && Habbo.Rank == 1 &&
                (Habbo.LastNameChange == 0 || OblivionServer.GetUnixTimestamp() + 604800 > Habbo.LastNameChange))
                return true;
            if (Habbo.Rank == 1 && Habbo.Rank == 2 &&
                (Habbo.LastNameChange == 0 || OblivionServer.GetUnixTimestamp() + 86400 > Habbo.LastNameChange))
                return true;
            if (Habbo.Rank == 1 && Habbo.Rank == 3)
                return true;
            return Habbo.GetPermissions().HasRight("mod_tool");
        }
    }
}