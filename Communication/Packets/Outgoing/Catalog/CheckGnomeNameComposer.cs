﻿namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    internal class CheckGnomeNameComposer : ServerPacket
    {
        public CheckGnomeNameComposer(string PetName, int ErrorId)
            : base(ServerPacketHeader.CheckGnomeNameMessageComposer)
        {
            WriteInteger(0);
            WriteInteger(ErrorId);
            WriteString(PetName);
        }
    }
}