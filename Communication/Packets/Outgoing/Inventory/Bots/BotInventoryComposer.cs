#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Users.Inventory.Bots;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Inventory.Bots
{
    internal class BotInventoryComposer : ServerPacket
    {
        public BotInventoryComposer(ICollection<Bot> Bots)
            : base(ServerPacketHeader.BotInventoryMessageComposer)
        {
            WriteInteger(Bots.Count);
            foreach (var Bot in Bots.ToList())
            {
                WriteInteger(Bot.Id);
                WriteString(Bot.Name);
                WriteString(Bot.Motto);
                WriteString(Bot.Gender);
                WriteString(Bot.Figure);
            }
        }
    }
}