#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Navigator;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class NavigatorMetaDataParserComposer : ServerPacket
    {
        public NavigatorMetaDataParserComposer(ICollection<TopLevelItem> TopLevelItems)
            : base(ServerPacketHeader.NavigatorMetaDataParserMessageComposer)
        {
            WriteInteger(TopLevelItems.Count); //Count
            foreach (var TopLevelItem in TopLevelItems.ToList())
            {
                //TopLevelContext
                WriteString(TopLevelItem.SearchCode); //Search code
                WriteInteger(0); //Count of saved searches?
                /*{
                    //SavedSearch
                    base.WriteInteger(TopLevelItem.Id);//Id
                   base.WriteString(TopLevelItem.SearchCode);//Search code
                   base.WriteString(TopLevelItem.Filter);//Filter
                   base.WriteString(TopLevelItem.Localization);//localization
                }*/
            }
        }
    }
}