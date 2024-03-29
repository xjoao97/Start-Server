﻿#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class GetClubGiftsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(new ClubGiftsComposer());
    }
}