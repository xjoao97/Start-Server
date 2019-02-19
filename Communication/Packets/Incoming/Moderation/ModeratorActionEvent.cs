#region

using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ModeratorActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_caution"))
                return;

            if (!Session.GetHabbo().InRoom)
                return;

            var CurrentRoom = Session.GetHabbo().CurrentRoom;
            if (CurrentRoom == null)
                return;

            var AlertMode = Packet.PopInt();
            var AlertMessage = Packet.PopString();
            var IsCaution = AlertMode != 3;

            AlertMessage = IsCaution
                ? "Caution from Moderator:\n\n" + AlertMessage
                : "Message from Moderator:\n\n" + AlertMessage;
            Session.GetHabbo().CurrentRoom.SendMessage(new BroadcastMessageAlertComposer(AlertMessage));
        }
    }
}