#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Games;

#endregion

namespace Oblivion.Communication.Packets.Incoming.GameCenter
{
    internal class Game2GetWeeklyLeaderboardEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GameId = Packet.PopInt();

            GameData GameData = null;
            if (OblivionServer.GetGame().GetGameDataManager().TryGetGame(GameId, out GameData))
            {
                //Code
            }
        }
    }
}