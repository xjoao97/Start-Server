#region

using Oblivion.HabboHotel.Users.Effects;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    internal class AvatarEffectActivatedComposer : ServerPacket
    {
        public AvatarEffectActivatedComposer(AvatarEffect Effect)
            : base(ServerPacketHeader.AvatarEffectActivatedMessageComposer)
        {
            WriteInteger(Effect.SpriteId);
            WriteInteger((int) Effect.Duration);
            WriteBoolean(false); //Permanent
        }
    }
}