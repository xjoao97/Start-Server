﻿using System.Web;
using Oblivion.Utilities;

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Notifications
{
    internal class RoomNotificationComposer : ServerPacket
    {
        public RoomNotificationComposer(string Type, string Key, string Value)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            WriteString(Type);
            WriteInteger(1); //Count
            {
                WriteString(Key); //Type of message
                WriteString(Value);
            }
        }

        public RoomNotificationComposer(string Type)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            WriteString(Type);
            WriteInteger(0); //Count
        }

        public RoomNotificationComposer(string Title, string Message, string Image, string HotelName,
            string HotelURL, bool isBubble = false)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            // ConsoleWriter.Writer.WriteLine(Image);
            Message = HttpUtility.HtmlDecode(Message);
            WriteString(Image);
            WriteInteger(5);
            WriteString("title");
            WriteString(Title);
            WriteString("message");
            WriteString(Message);
            //Image.Replace("_", "-");
            WriteString("linkUrl");
            WriteString(HotelURL);
            WriteString("linkTitle");
            WriteString(HotelName);

            WriteString("display");
            WriteString(isBubble ? "BUBBLE" : "POP_UP");
        }

        public static ServerPacket SendBubble(string image, string message, string linkUrl = "")
        {
            var bubbleNotification = new ServerPacket(ServerPacketHeader.RoomNotificationMessageComposer);
            bubbleNotification.WriteString(image);
            bubbleNotification.WriteInteger(string.IsNullOrEmpty(linkUrl) ? 2 : 3);
            bubbleNotification.WriteString("display");
            bubbleNotification.WriteString("BUBBLE");
            bubbleNotification.WriteString("message");
            bubbleNotification.WriteString(message);
            if (string.IsNullOrEmpty(linkUrl)) return bubbleNotification;
            bubbleNotification.WriteString("linkUrl");
            bubbleNotification.WriteString(linkUrl);
            return bubbleNotification;
        }

        public RoomNotificationComposer(string Title, string Message, string Image, string HotelName = "",
            string HotelURL = "")
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            WriteString(Image);
            WriteInteger(string.IsNullOrEmpty(HotelName) ? 2 : 4);
            WriteString("title");
            WriteString(Title);
            WriteString("message");
            WriteString(Message);

            if (string.IsNullOrEmpty(HotelName)) return;
            WriteString("linkUrl");
            WriteString(HotelURL);
            WriteString("linkTitle");
            WriteString(HotelName);
        }
    }
}