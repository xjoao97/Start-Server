#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Connection
{
    internal class GoToFlatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;


            if (!Session.GetHabbo().EnterRoom(Session.GetHabbo().CurrentRoom))
                Session.SendMessage(new CloseConnectionComposer());
        }
    }
}