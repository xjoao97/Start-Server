using Oblivion.Communication.Packets.Outgoing;

namespace Plus.Communication.Packets.Outgoing.LandingView
{
    internal class CampaignCalendarGiftComposer : ServerPacket
    {
        public CampaignCalendarGiftComposer(string iconName = "throne")
            : base(ServerPacketHeader.CampaignCalendarGiftMessageComposer)
        {
            WriteBoolean(true); // never bothered to check
            WriteString("xmas14_starfish"); //productName
            WriteString(""); //customImage
            WriteString(iconName); //iconName
        }
    }
}