#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ModerateRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            Room Room;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Packet.PopInt(), out Room))
                return;

            var SetLock = Packet.PopInt() == 1;
            var SetName = Packet.PopInt() == 1;
            var KickAll = Packet.PopInt() == 1;

            if (SetName)
            {
                Room.RoomData.Name = "Quarto inapropriado.";
                Room.RoomData.Description = "Inapropriado.";
            }

            if (SetLock)
                Room.RoomData.Access = RoomAccess.DOORBELL;

            if (Room.Tags.Count > 0)
                Room.ClearTags();

            if (Room.RoomData.HasActivePromotion)
                Room.RoomData.EndPromotion();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                if (SetName && SetLock)
                    dbClient.RunQuery(
                        "UPDATE `rooms` SET `caption` = 'Quarto inapropriado', `description` = 'Inapropriado.', `tags` = '', `state` = '1' WHERE `id` = '" +
                        Room.RoomId + "' LIMIT 1");
                else if (SetName)
                    dbClient.RunQuery(
                        "UPDATE `rooms` SET `caption` = 'Quarto inapropriado', `description` = 'Inapropriado.', `tags` = '' WHERE `id` = '" +
                        Room.RoomId + "' LIMIT 1");
                else if (SetLock)
                    dbClient.RunQuery("UPDATE `rooms` SET `state` = '1', `tags` = '' WHERE `id` = '" + Room.RoomId +
                                      "' LIMIT 1");
            }

            Room.SendMessage(new RoomSettingsSavedComposer(Room.RoomId));
            Room.SendMessage(new RoomInfoUpdatedComposer(Room.RoomId));

            if (KickAll)
                foreach (var RoomUser in from RoomUser in Room.GetRoomUserManager().GetUserList().ToList()
                    where RoomUser != null && !RoomUser.IsBot
                    where RoomUser.GetClient() != null && RoomUser.GetClient().GetHabbo() != null
                    where
                    RoomUser.GetClient().GetHabbo().Rank < Session.GetHabbo().Rank &&
                    RoomUser.GetClient().GetHabbo().Id != Session.GetHabbo().Id
                    select RoomUser)
                    Room.GetRoomUserManager().RemoveUserFromRoom(RoomUser.GetClient(), true);
        }
    }
}