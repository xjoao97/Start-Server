#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Games;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.GameCenter
{
    internal class GameListComposer : ServerPacket
    {
        public GameListComposer(IEnumerable<GameData> Games)
            : base(ServerPacketHeader.GameListMessageComposer)
        {
            WriteInteger(OblivionServer.GetGame().GetGameDataManager().GetCount()); //Game count
            foreach (var Game in Games)
            {
                WriteInteger(Game.GameId);
                WriteString(Game.GameName);
                WriteString(Game.ColourOne);
                WriteString(Game.ColourTwo);
                WriteString(Game.ResourcePath);
                WriteString(Game.StringThree);
            }
        }
    }
}