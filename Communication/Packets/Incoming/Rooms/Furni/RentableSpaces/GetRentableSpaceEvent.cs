#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.RentableSpaces;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    internal class GetRentableSpaceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Something = Packet.PopInt();
            Session.SendMessage(new RentableSpaceComposer());
        }
    }
}