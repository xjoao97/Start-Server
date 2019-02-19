#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Navigator;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class UserFlatCatsComposer : ServerPacket
    {
        public UserFlatCatsComposer(ICollection<SearchResultList> Categories, int Rank)
            : base(ServerPacketHeader.UserFlatCatsMessageComposer)
        {
            WriteInteger(Categories.Count);
            foreach (var Cat in Categories)
            {
                WriteInteger(Cat.Id);
                WriteString(Cat.PublicName);
                WriteBoolean(Cat.RequiredRank <= Rank);
                WriteBoolean(false);
                WriteString("");
                WriteString("");
                WriteBoolean(false);
            }
        }
    }
}