#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.AvatarEffects
{
    internal class AvatarEffectSelectedEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var EffectId = Packet.PopInt();
            if (EffectId < 0)
                EffectId = 0;

            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (EffectId != 0 && Session.GetHabbo().Effects().HasEffect(EffectId, true))
                User.ApplyEffect(EffectId);
        }
    }
}