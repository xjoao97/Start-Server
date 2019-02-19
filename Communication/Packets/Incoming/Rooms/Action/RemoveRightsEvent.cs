#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Permissions;
using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class RemoveRightsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session, true))
                return;

            var Amount = Packet.PopInt();
            for (var i = 0; i < Amount && i <= 100; i++)
            {
                var UserId = Packet.PopInt();
                if (UserId > 0 && Room.UsersWithRights.Contains(UserId))
                {
                    var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                    if (User != null && !User.IsBot)
                    {
                        User.RemoveStatus("flatctrl 1");
                        User.UpdateNeeded = true;


                        User.GetClient().SendMessage(new YouAreControllerComposer(0));
                    }

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery(
                            "DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                        dbClient.AddParameter("uid", UserId);
                        dbClient.AddParameter("rid", Room.Id);
                        dbClient.RunQuery();
                    }

                    if (Room.UsersWithRights.Contains(UserId))
                        Room.UsersWithRights.Remove(UserId);

                    Session.SendMessage(new FlatControllerRemovedComposer(Room, UserId));
                }
            }
        }
    }
}