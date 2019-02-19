﻿namespace Oblivion.Communication.Packets.Outgoing.Inventory.Achievements
{
    internal class AchievementScoreComposer : ServerPacket
    {
        public AchievementScoreComposer(int achScore)
            : base(ServerPacketHeader.AchievementScoreMessageComposer)
        {
            WriteInteger(achScore);
        }
    }
}