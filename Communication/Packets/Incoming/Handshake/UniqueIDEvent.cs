#region

using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Handshake
{
    public class UniqueIDEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Junk = Packet.PopString();
            var MachineId = Packet.PopString();

            Session.MachineId = MachineId;

            Session.SendMessage(new SetUniqueIdComposer(MachineId));
        }
    }
}