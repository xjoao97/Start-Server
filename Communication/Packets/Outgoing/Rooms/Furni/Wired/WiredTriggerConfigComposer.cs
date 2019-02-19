#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Wired;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Furni.Wired
{
    internal class WiredTriggerConfigComposer : ServerPacket
    {
        public WiredTriggerConfigComposer(IWiredItem Box, List<int> BlockedItems)
            : base(ServerPacketHeader.WiredTriggerConfigMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(5);

            WriteInteger(Box.SetItems.Count);
            foreach (var Item in Box.SetItems.Values.ToList())
                WriteInteger(Item.Id);

            WriteInteger(Box.Item.GetBaseItem().SpriteId);
            WriteInteger(Box.Item.Id);
            WriteString(Box.StringData);

            WriteInteger(Box is IWiredCycle ? 1 : 0);
            if (Box is IWiredCycle && Box.Type != WiredBoxType.TriggerLongRepeat)
            {
                var Cycle = (IWiredCycle) Box;
                WriteInteger(Convert.ToInt32(Cycle.Delay));
            }
            else if (Box.Type == WiredBoxType.TriggerLongRepeat)
            {
                var Cycle = (IWiredCycle) Box;
                WriteInteger(Convert.ToInt32(Cycle.Delay * 500 / 5000));
            }


            WriteInteger(0);
            WriteInteger(WiredBoxTypeUtility.GetWiredId(Box.Type));
            WriteInteger(BlockedItems.Count);
            if (BlockedItems.Any())
                foreach (var Id in BlockedItems.ToList())
                    WriteInteger(Id);
        }
    }
}