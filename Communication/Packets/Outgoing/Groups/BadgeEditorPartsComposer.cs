#region

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class BadgeEditorPartsComposer : ServerPacket
    {
        public BadgeEditorPartsComposer(ICollection<GroupBases> Bases, ICollection<GroupSymbols> Symbols,
            ICollection<GroupBaseColours> BaseColours, HybridDictionary SymbolColours,
            HybridDictionary BackgroundColours)
            : base(ServerPacketHeader.BadgeEditorPartsMessageComposer)
        {
            WriteInteger(Bases.Count);
            foreach (var Item in Bases)
            {
                WriteInteger(Item.Id);
                WriteString(Item.Value1);
                WriteString(Item.Value2);
            }

            WriteInteger(Symbols.Count);
            foreach (var Item in Symbols)
            {
                WriteInteger(Item.Id);
                WriteString(Item.Value1);
                WriteString(Item.Value2);
            }

            WriteInteger(BaseColours.Count);
            foreach (var Colour in BaseColours)
            {
                WriteInteger(Colour.Id);
                WriteString(Colour.Colour);
            }

            WriteInteger(SymbolColours.Count);
            foreach (var Colour in SymbolColours.Values.Cast<GroupSymbolColours>())
            {
                WriteInteger(Colour.Id);
                WriteString(Colour.Colour);
            }

            WriteInteger(BackgroundColours.Count);
            foreach (var Colour in BackgroundColours.Values.Cast<GroupBackGroundColours>())
            {
                WriteInteger(Colour.Id);
                WriteString(Colour.Colour);
            }
        }
    }
}