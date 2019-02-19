#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Games;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.GameCenter
{
    internal class Game1WeeklyLeaderboardComposer : ServerPacket
    {
        public Game1WeeklyLeaderboardComposer(GameData GameData, ICollection<Habbo> Habbos)
            : base(ServerPacketHeader.Game1WeeklyLeaderboardMessageComposer)
        {
            WriteInteger(2014);
            WriteInteger(41);
            WriteInteger(0);
            WriteInteger(1);
            WriteInteger(1581);

            //Used to generate the ranking numbers.
            var num = 0;

            WriteInteger(Habbos.Count); //Count
            foreach (var Habbo in Habbos.ToList())
            {
                num++;
                WriteInteger(Habbo.Id); //Id
                WriteInteger(Habbo.FastfoodScore); //Score
                WriteInteger(num); //Rank
                WriteString(Habbo.Username); //Username
                WriteString(Habbo.Look); //Figure
                WriteString(Habbo.Gender.ToLower()); //Gender .ToLower()
            }

            WriteInteger(1); //
            WriteInteger(GameData.GameId); //Game Id?
        }
    }
}