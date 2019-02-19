#region

using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorUserChatlogEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var UserId = Packet.PopInt();
            var Habbo = OblivionServer.GetHabboById(UserId);

            if (Habbo == null)
            {
                Session.SendNotification("Oops, we couldn't find this user.");
                return;
            }

            try
            {
                Session.SendMessage(new ModeratorUserChatlogComposer(UserId));
            }
            catch
            {
                Session.SendNotification("Overflow :/");
            }
        }
    }
}