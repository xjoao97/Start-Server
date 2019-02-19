#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ModerationKickEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_kick"))
                return;

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client?.GetHabbo() == null || Client.GetHabbo().CurrentRoomId < 1 ||
                Client.GetHabbo().Id == Session.GetHabbo().Id)
                return;

            if (Client.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendNotification(
                    OblivionServer.GetGame().GetLanguageLocale().TryGetValue("moderation_kick_permissions"));
                return;
            }

            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            Room.GetRoomUserManager().RemoveUserFromRoom(Client, true);
        }
    }
}