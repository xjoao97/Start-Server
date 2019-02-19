#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ModerationMsgEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_alert"))
                return;

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null)
                return;

            Client.SendNotification(Message);
        }
    }
}