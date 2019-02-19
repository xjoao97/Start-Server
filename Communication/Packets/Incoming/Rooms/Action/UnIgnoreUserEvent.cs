#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Action;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class UnIgnoreUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var Username = Packet.PopString();

            var User = OblivionServer.GetHabboByUsername(Username);
            if (User == null || !Session.GetHabbo().MutedUsers.Contains(User.Id))
                return;

            Session.GetHabbo().MutedUsers.Remove(User.Id);
            Session.SendMessage(new IgnoreStatusComposer(3, Username));
        }
    }
}