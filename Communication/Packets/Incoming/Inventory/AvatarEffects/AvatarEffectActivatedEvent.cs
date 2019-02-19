#region

using Oblivion.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.AvatarEffects
{
    internal class AvatarEffectActivatedEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var EffectId = Packet.PopInt();

            var Effect = Session.GetHabbo().Effects().GetEffectNullable(EffectId, false, true);

            if (Effect == null || Session.GetHabbo().Effects().HasEffect(EffectId, true))
                return;

            if (Effect.Activate())
                Session.SendMessage(new AvatarEffectActivatedComposer(Effect));
        }
    }
}