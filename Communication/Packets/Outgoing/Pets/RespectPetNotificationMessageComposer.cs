#region

using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Pets
{
    internal class RespectPetNotificationMessageComposer : ServerPacket
    {
        public RespectPetNotificationMessageComposer(Pet Pet)
            : base(ServerPacketHeader.RespectPetNotificationMessageComposer)
        {
            WriteInteger(Pet.VirtualId);
            WriteInteger(Pet.VirtualId);
            WriteInteger(Pet.PetId); //Pet Id, 100%
            WriteString(Pet.Name);
            WriteInteger(0);
            WriteInteger(0);
            WriteString(Pet.Color);
            WriteInteger(0);
            WriteInteger(0); //Count - 3 ints.
            WriteInteger(1);
        }

        public RespectPetNotificationMessageComposer(Habbo Habbo, RoomUser User)
            : base(ServerPacketHeader.RespectPetNotificationMessageComposer)
        {
            WriteInteger(User.VirtualId);
            WriteInteger(User.VirtualId);
            WriteInteger(Habbo.Id); //Pet Id, 100%
            WriteString(Habbo.Username);
            WriteInteger(0);
            WriteInteger(0);
            WriteString("FFFFFF"); //Yeah..
            WriteInteger(0);
            WriteInteger(0); //Count - 3 ints.
            WriteInteger(1);
        }
    }
}