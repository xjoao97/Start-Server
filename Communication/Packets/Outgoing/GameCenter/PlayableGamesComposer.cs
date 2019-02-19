﻿namespace Oblivion.Communication.Packets.Outgoing.GameCenter
{
    internal class PlayableGamesComposer : ServerPacket
    {
        public PlayableGamesComposer(int GameID)
            : base(ServerPacketHeader.PlayableGamesMessageComposer)
        {
            WriteInteger(GameID);
            WriteInteger(0);
        }
    }
}