#region

using Oblivion.Communication.Packets.Outgoing.Inventory.Pets;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Pets
{
    internal class GetPetInventoryEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
                return;

            var Pets = Session.GetHabbo().GetInventoryComponent().GetPets();
            Session.SendMessage(new PetInventoryComposer(Pets));
        }
    }
}