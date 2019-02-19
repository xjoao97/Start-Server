#region

using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorRoomChatlogEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var Junk = Packet.PopInt();

            Room Room;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Packet.PopInt(), out Room))
                return;

            try
            {
                Session.SendMessage(new ModeratorRoomChatlogComposer(Room));
            }
            catch
            {
                Session.SendNotification("Overflow :/");
            }
        }
    }
}