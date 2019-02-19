#region

using Oblivion.Communication.Packets.Outgoing.GameCenter;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.GameCenter
{
    internal class GetPlayableGamesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GameId = Packet.PopInt();

            Session.SendMessage(new GameAccountStatusComposer(GameId));
            Session.SendMessage(new PlayableGamesComposer(GameId));
            Session.SendMessage(new GameAchievementListComposer(Session,
                OblivionServer.GetGame().GetAchievementManager().GetGameAchievements(GameId), GameId));
        }
    }
}