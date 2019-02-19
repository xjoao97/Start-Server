#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Wired;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Furni.Wired
{
    internal class WiredEffectConfigComposer : ServerPacket
    {
        public WiredEffectConfigComposer(IWiredItem Box, List<int> BlockedItems)
            : base(ServerPacketHeader.WiredEffectConfigMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(15);

            WriteInteger(Box.SetItems.Count);
            foreach (var Item in Box.SetItems.Values.ToList())
                WriteInteger(Item.Id);

            WriteInteger(Box.Item.GetBaseItem().SpriteId);
            WriteInteger(Box.Item.Id);

            if (Box.Type == WiredBoxType.EffectBotGivesHanditemBox)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                    Box.StringData = "Bot name;0";

                WriteString(Box.StringData != null ? Box.StringData.Split(';')[0] : "");
            }
            else if (Box.Type == WiredBoxType.EffectBotFollowsUserBox)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                    Box.StringData = "0;Bot name";

                WriteString(Box.StringData != null ? Box.StringData.Split(';')[1] : "");
            }
            else if (Box.Type == WiredBoxType.EffectGiveReward || Box.Type == WiredBoxType.EffectGiveUserCredits)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                    Box.StringData = "1,,;1,,;1,,;1,,;1,,-0-0";

                WriteString(Box.StringData != null ? Box.StringData.Split('-')[0] : "");
            }
            else
            {
                WriteString(Box.StringData);
            }

            if (Box.Type != WiredBoxType.EffectMatchPosition && Box.Type != WiredBoxType.EffectMoveAndRotate &&
                Box.Type != WiredBoxType.EffectMuteTriggerer && Box.Type != WiredBoxType.EffectBotFollowsUserBox &&
                Box.Type != WiredBoxType.EffectGiveReward && Box.Type != WiredBoxType.EffectAddScore)
            {
                WriteInteger(0); // Loop
            }
            else if (Box.Type == WiredBoxType.EffectMatchPosition)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                    Box.StringData = "0;0;0";

                WriteInteger(3);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 0);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[1]) : 0);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[2]) : 0);
            }

            else if (Box.Type == WiredBoxType.EffectAddScore)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                    Box.StringData = "1;1";

                WriteInteger(2);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 0);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[1]) : 0);
            }
            else if (Box.Type == WiredBoxType.EffectGiveReward)
            {
                WriteInteger(3);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split('-')[1]) : 0);
                WriteInteger(Box.BoolData ? 1 : 0);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split('-')[2]) : 0);
            }
            else if (Box.Type == WiredBoxType.EffectMoveAndRotate)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                    Box.StringData = "0;0";

                WriteInteger(2);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 0);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[1]) : 0);
            }
            else if (Box.Type == WiredBoxType.EffectMuteTriggerer)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                    Box.StringData = "0;Message";

                WriteInteger(1); //Count, for the time.
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 0);
            }
            else if (Box.Type == WiredBoxType.EffectBotFollowsUserBox)
            {
                WriteInteger(1); //Count, for the time.
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 0);
            }
            else if (Box.Type == WiredBoxType.EffectBotGivesHanditemBox)
            {
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[1]) : 0);
            }

            var box = Box as IWiredCycle;
            if (box != null && Box.Type != WiredBoxType.EffectKickUser &&
                Box.Type != WiredBoxType.EffectMatchPosition && Box.Type != WiredBoxType.EffectMoveAndRotate &&
                Box.Type != WiredBoxType.EffectSetRollerSpeed && Box.Type != WiredBoxType.EffectAddScore)
            {
                var Cycle = box;
                WriteInteger(WiredBoxTypeUtility.GetWiredId(Box.Type));
                WriteInteger(0);
                WriteInteger(Convert.ToInt32(Cycle.Delay));
            }
            else if (Box.Type == WiredBoxType.EffectMatchPosition ||
                     Box.Type == WiredBoxType.EffectMoveAndRotate || Box.Type == WiredBoxType.EffectAddScore)
            {
                var Cycle = (IWiredCycle) Box;
                WriteInteger(0);
                WriteInteger(WiredBoxTypeUtility.GetWiredId(Box.Type));
                WriteInteger(Convert.ToInt32(Cycle.Delay));
            }
            else
            {
                WriteInteger(0);
                WriteInteger(WiredBoxTypeUtility.GetWiredId(Box.Type));
                WriteInteger(0);
            }

            WriteInteger(BlockedItems.Count); // Incompatable items loop
            if (!BlockedItems.Any()) return;
            foreach (var ItemId in BlockedItems.ToList())
                WriteInteger(ItemId);
        }
    }
}