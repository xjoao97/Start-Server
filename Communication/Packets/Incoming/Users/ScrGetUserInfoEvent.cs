#region

using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class ScrGetUserInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(new ScrSendUserInfoComposer());
    }
}