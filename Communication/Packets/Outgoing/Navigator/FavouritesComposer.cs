#region

using System.Collections;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class FavouritesComposer : ServerPacket
    {
        public FavouritesComposer(ArrayList favouriteIDs)
            : base(ServerPacketHeader.FavouritesMessageComposer)
        {
            WriteInteger(50);
            WriteInteger(favouriteIDs.Count);

            foreach (int Id in favouriteIDs.ToArray())
                WriteInteger(Id);
        }
    }
}