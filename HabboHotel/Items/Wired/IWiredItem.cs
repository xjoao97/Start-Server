#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired
{
    public interface IWiredItem
    {
        Room Instance { get; set; }
        Item Item { get; set; }
        WiredBoxType Type { get; }
        ConcurrentDictionary<int, Item> SetItems { get; set; }
        string StringData { get; set; }
        bool BoolData { get; set; }
        string ItemsData { get; set; }
        void HandleSave(ClientPacket Packet);
        bool Execute(params object[] Params);
    }
}