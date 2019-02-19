#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class GetSellablePetBreedsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Type = Packet.PopString();
            var Item = OblivionServer.GetGame().GetItemManager().GetItembyName(Type);
            if (Item == null)
                return;

            var PetId = Item.BehaviourData;

            Session.SendMessage(new SellablePetBreedsComposer(Type, PetId, OblivionServer.GetGame().GetCatalog().GetPetRaceManager().GetRacesForRaceId(PetId)));
        }
    }
}