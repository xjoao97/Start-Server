using System;
using System.Collections.Generic;
using System.Linq;

namespace Oblivion.Communication.Packets.Outgoing.Campaigns
{
    internal class CampaignCalendarDataComposer : ServerPacket
    {
        public CampaignCalendarDataComposer(List<int> OpenedGifts)
            : base(ServerPacketHeader.CampaignCalendarDataMessageComposer)
        {
            var currentDate = DateTime.Now.Day - 1;

            WriteString("xmas16"); //eventTrigger
            WriteString(string.Empty); //idk? same as habbo ;P
            WriteInteger(currentDate); //currentDate
            WriteInteger(25); //totalAmountOfDays

            WriteInteger(OpenedGifts.Count); //countOpenGifts
            foreach (var Opened in OpenedGifts)
                WriteInteger(Opened); //giftDay

            var MissedGifts = Enumerable.Range(0, currentDate - 2).Where(Day => !OpenedGifts.Contains(Day)).ToList();

            WriteInteger(MissedGifts.Count); //countMissedGifts
            foreach (var Missed in MissedGifts)
                WriteInteger(Missed); //giftDay
        }
    }
}