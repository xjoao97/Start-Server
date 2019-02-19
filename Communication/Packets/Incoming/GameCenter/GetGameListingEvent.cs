#region

using Oblivion.Communication.Packets.Outgoing.GameCenter;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.GameCenter
{
    internal class GetGameListingEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Games = OblivionServer.GetGame().GetGameDataManager().GameData;

            Session.SendMessage(new GameListComposer(Games));
        }
    }
}