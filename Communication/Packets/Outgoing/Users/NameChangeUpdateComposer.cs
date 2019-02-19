#region

using System.Collections.Generic;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class NameChangeUpdateComposer : ServerPacket
    {
        public NameChangeUpdateComposer(string Name, int Error, ICollection<string> Tags)
            : base(ServerPacketHeader.NameChangeUpdateMessageComposer)
        {
            WriteInteger(Error);
            WriteString(Name);

            WriteInteger(Tags.Count);
            foreach (var Tag in Tags)
                WriteString(Name + Tag);
        }

        public NameChangeUpdateComposer(string Name, int Error)
            : base(ServerPacketHeader.NameChangeUpdateMessageComposer)
        {
            WriteInteger(Error);
            WriteString(Name);
            WriteInteger(0);
        }
    }
}