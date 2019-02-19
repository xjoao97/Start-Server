#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class CheckPetNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PetName = Packet.PopString();

            if (PetName.Length < 2)
            {
                Session.SendMessage(new CheckPetNameComposer(2, "2"));
                return;
            }
            if (PetName.Length > 15)
            {
                Session.SendMessage(new CheckPetNameComposer(1, "15"));
                return;
            }
            if (!OblivionServer.IsValidAlphaNumeric(PetName))
            {
                Session.SendMessage(new CheckPetNameComposer(3, ""));
                return;
            }
            if (OblivionServer.GetGame().GetChatManager().GetFilter().IsFiltered(PetName))
            {
                Session.SendMessage(new CheckPetNameComposer(4, ""));
                return;
            }

            Session.SendMessage(new CheckPetNameComposer(0, ""));
        }
    }
}