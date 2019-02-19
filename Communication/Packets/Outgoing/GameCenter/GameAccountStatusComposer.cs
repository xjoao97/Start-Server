﻿namespace Oblivion.Communication.Packets.Outgoing.GameCenter
{
    internal class GameAccountStatusComposer : ServerPacket
    {
        public GameAccountStatusComposer(int GameID)
            : base(ServerPacketHeader.GameAccountStatusMessageComposer)
        {
            WriteInteger(GameID);
            WriteInteger(-1); // Games Left
            WriteInteger(0); //Was 16?
        }
    }
}