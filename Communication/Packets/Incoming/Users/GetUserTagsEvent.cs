#region

using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class GetUserTagsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var UserId = Packet.PopInt();

            Session.SendMessage(new UserTagsComposer(UserId));
        }
    }
}