﻿#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.FloorPlan
{
    internal class FloorPlanFloorMapComposer : ServerPacket
    {
        public FloorPlanFloorMapComposer(ICollection<Item> Items)
            : base(ServerPacketHeader.FloorPlanFloorMapMessageComposer)
        {
            WriteInteger(Items.Count);
            //TODO: Figure this out, it pushes the room coords, but it iterates them, x,y|x,y|x,y|and so on.
            foreach (var Item in Items.ToList())
            {
                WriteInteger(Item.GetX);
                WriteInteger(Item.GetY);
            }
        }
    }
}