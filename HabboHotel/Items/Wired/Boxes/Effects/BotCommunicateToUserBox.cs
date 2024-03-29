﻿using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    class BotCommunicateToUserBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.EffectBotCommunicatesToUserBox; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotCommunicateToUserBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            int Unknown = Packet.PopInt();
            int ChatMode = Packet.PopInt();
            string ChatConfig = Packet.PopString();

            this.StringData = ChatConfig;
            if (ChatMode == 1)
            {
                this.BoolData = true;
            }
            else
            {
                this.BoolData = false;
            }

        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            if (String.IsNullOrEmpty(this.StringData))
                return false;

            this.StringData.Split(' ');
            string BotName = this.StringData.Split('	')[0];
            string Chat = this.StringData.Split('	')[1];

            string Message = StringData.Split('	')[1];
            string MessageFiltered = StringData.Split('	')[1];

            RoomUser User = this.Instance.GetRoomUserManager().GetBotByName(BotName);
            if (User == null)
                return false;

            Habbo Player = (Habbo)Params[0];
            if (this.BoolData)
            {
                Player.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Chat, 0, 31));
            }
            else
            {
                User.Chat(Player.GetClient().GetHabbo().Username + ": " + Chat, false, User.LastBubble);
            }

            return true;
        }
    }
}