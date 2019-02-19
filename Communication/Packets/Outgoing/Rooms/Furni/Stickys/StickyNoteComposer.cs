﻿namespace Oblivion.Communication.Packets.Outgoing.Rooms.Furni.Stickys
{
    internal class StickyNoteComposer : ServerPacket
    {
        public StickyNoteComposer(string ItemId, string Extradata)
            : base(ServerPacketHeader.StickyNoteMessageComposer)
        {
            WriteString(ItemId);
            WriteString(Extradata);
        }
    }
}