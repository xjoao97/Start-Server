#region

using System;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class MutedComposer : ServerPacket
    {
        public MutedComposer(double TimeMuted)
            : base(ServerPacketHeader.MutedMessageComposer)
        {
            WriteInteger(Convert.ToInt32(TimeMuted));
        }
    }
}