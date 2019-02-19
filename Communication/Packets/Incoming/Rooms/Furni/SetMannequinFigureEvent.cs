#region

using System;
using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class SetMannequinFigureEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var ItemId = Packet.PopInt();
            var Item = Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            var Gender = Session.GetHabbo().Gender.ToLower();
            var Figure = "";

            foreach (var Str in Session.GetHabbo().Look.Split('.'))
            {
                if (Str.Contains("hr") || Str.Contains("hd") || Str.Contains("he") || Str.Contains("ea") ||
                    Str.Contains("ha"))
                    continue;

                Figure += Str + ".";
            }

            Figure = Figure.TrimEnd('.');
            if (Item.ExtraData.Contains(Convert.ToChar(5)))
            {
                var Flags = Item.ExtraData.Split(Convert.ToChar(5));
                Item.ExtraData = Gender + Convert.ToChar(5) + Figure + Convert.ToChar(5) + Flags[2];
            }
            else
            {
                Item.ExtraData = Gender + Convert.ToChar(5) + Figure + Convert.ToChar(5) + "Default";
            }

            Item.UpdateState(true, true);
        }
    }
}