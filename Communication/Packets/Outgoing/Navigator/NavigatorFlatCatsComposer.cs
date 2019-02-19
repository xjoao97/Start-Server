#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Navigator;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class NavigatorFlatCatsComposer : ServerPacket
    {
        public NavigatorFlatCatsComposer(ICollection<SearchResultList> Categories)
            : base(ServerPacketHeader.NavigatorFlatCatsMessageComposer)
        {
            WriteInteger(Categories.Count);
            foreach (var Category in Categories.ToList())
            {
                WriteInteger(Category.Id);
                WriteString(Category.PublicName);
                WriteBoolean(true); //TODO
            }
        }
    }
}