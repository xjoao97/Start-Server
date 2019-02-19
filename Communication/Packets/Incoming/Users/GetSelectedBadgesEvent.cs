#region

using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class GetSelectedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var UserId = Packet.PopInt();
            var Habbo = OblivionServer.GetHabboById(UserId);
            if (Habbo == null)
                return;

            Session.SendMessage(new HabboUserBadgesComposer(Habbo));
        }
    }
}