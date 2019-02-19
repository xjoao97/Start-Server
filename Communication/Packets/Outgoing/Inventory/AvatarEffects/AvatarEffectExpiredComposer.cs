#region

using Oblivion.HabboHotel.Users.Effects;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    internal class AvatarEffectExpiredComposer : ServerPacket
    {
        public AvatarEffectExpiredComposer(AvatarEffect Effect)
            : base(ServerPacketHeader.AvatarEffectExpiredMessageComposer)
        {
            WriteInteger(Effect.SpriteId);
        }
    }
}