#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Permissions;
using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class RemoveAllRightsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Instance;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Instance))
                return;

            if (!Instance.CheckRights(Session, true))
                return;

            foreach (var UserId in new List<int>(Instance.UsersWithRights))
            {
                var User = Instance.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (User != null && !User.IsBot)
                {
                    User.RemoveStatus("flatctrl 1");
                    User.UpdateNeeded = true;

                    User.GetClient().SendMessage(new YouAreControllerComposer(0));
                }

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                    dbClient.AddParameter("uid", UserId);
                    dbClient.AddParameter("rid", Instance.Id);
                    dbClient.RunQuery();
                }

                Session.SendMessage(new FlatControllerRemovedComposer(Instance, UserId));
                Session.SendMessage(new RoomRightsListComposer(Instance));
                Session.SendMessage(new UserUpdateComposer(Instance.GetRoomUserManager().GetUserList().ToList()));
            }

            if (Instance.UsersWithRights.Count > 0)
                Instance.UsersWithRights.Clear();
        }
    }
}