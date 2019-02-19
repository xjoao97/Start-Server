﻿namespace Oblivion.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    internal class AvatarEffectAddedComposer : ServerPacket
    {
        public AvatarEffectAddedComposer(int SpriteId, int Duration)
            : base(ServerPacketHeader.AvatarEffectAddedMessageComposer)
        {
            WriteInteger(SpriteId);
            WriteInteger(0); //Types
            WriteInteger(Duration);
            WriteBoolean(false); //Permanent
        }
    }
}