#region

using Oblivion.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.AI.Pets
{
    internal class GetPetTrainingPanelEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var PetId = Packet.PopInt();

            RoomUser Pet;
            if (!Session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(PetId, out Pet))
            {
                //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
                var User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(PetId);

                //Check some values first, please!
                if (User?.GetClient() == null || User.GetClient().GetHabbo() == null)
                    return;

                //And boom! Let us send the training panel composer 8-).
                Session.SendWhisper("Maybe one day, boo boo.");
                return;
            }

            //Continue as a regular pet..
            if (Pet.RoomId != Session.GetHabbo().CurrentRoomId || Pet.PetData == null)
                return;

            Session.SendMessage(new PetTrainingPanelComposer(Pet.PetData.PetId, Pet.PetData.Level));
        }
    }
}