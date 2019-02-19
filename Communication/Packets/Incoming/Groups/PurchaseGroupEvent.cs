#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class PurchaseGroupEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().Credits < OblivionStaticGameSettings.GroupPurchaseAmount)
            {
                Session.SendMessage(
                    new BroadcastMessageAlertComposer("A group costs " + OblivionStaticGameSettings.GroupPurchaseAmount +
                                                      " credits! You only have " + Session.GetHabbo().Credits + "!"));
                return;
            }
            Session.GetHabbo().Credits -= OblivionStaticGameSettings.GroupPurchaseAmount;
            Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));

            var Name = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Description = OblivionServer.GetGame()
                .GetChatManager()
                .GetFilter()
                .CheckMessage(Packet.PopString());
            var RoomId = Packet.PopInt();
            var Colour1 = Packet.PopInt();
            var Colour2 = Packet.PopInt();
            var groupID3 = Packet.PopInt();
            var groupID4 = Packet.PopInt();
            var groupID5 = Packet.PopInt();
            var groupID6 = Packet.PopInt();
            var groupID7 = Packet.PopInt();
            var groupID8 = Packet.PopInt();
            var groupID9 = Packet.PopInt();
            var groupID10 = Packet.PopInt();
            var groupID11 = Packet.PopInt();
            var groupID12 = Packet.PopInt();
            var groupID13 = Packet.PopInt();
            var groupID14 = Packet.PopInt();
            var groupID15 = Packet.PopInt();
            var groupID16 = Packet.PopInt();
            var groupID17 = Packet.PopInt();
            var groupID18 = Packet.PopInt();

            var Room = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (Room == null || Room.OwnerId != Session.GetHabbo().Id || Room.Group != null)
                return;

            var Base = "b" + (groupID4 < 10 ? "0" + groupID4 : groupID4.ToString()) +
                       (groupID5 < 10 ? "0" + groupID5 : groupID5.ToString()) + groupID6;
            var Symbol1 = "s" + (groupID7 < 10 ? "0" + groupID7 : groupID7.ToString()) +
                          (groupID8 < 10 ? "0" + groupID8 : groupID8.ToString()) + groupID9;
            var Symbol2 = "s" + (groupID10 < 10 ? "0" + groupID10 : groupID10.ToString()) +
                          (groupID11 < 10 ? "0" + groupID11 : groupID11.ToString()) + groupID12;
            var Symbol3 = "s" + (groupID13 < 10 ? "0" + groupID13 : groupID13.ToString()) +
                          (groupID14 < 10 ? "0" + groupID14 : groupID14.ToString()) + groupID15;
            var Symbol4 = "s" + (groupID16 < 10 ? "0" + groupID16 : groupID16.ToString()) +
                          (groupID17 < 10 ? "0" + groupID17 : groupID17.ToString()) + groupID18;

            Symbol1 = OblivionServer.GetGame().GetGroupManager().CheckActiveSymbol(Symbol1);
            Symbol2 = OblivionServer.GetGame().GetGroupManager().CheckActiveSymbol(Symbol2);
            Symbol3 = OblivionServer.GetGame().GetGroupManager().CheckActiveSymbol(Symbol3);
            Symbol4 = OblivionServer.GetGame().GetGroupManager().CheckActiveSymbol(Symbol4);

            var Badge = Base + Symbol1 + Symbol2 + Symbol3 + Symbol4;

            Group Group;
            OblivionServer.GetGame()
                .GetGroupManager()
                .TryCreateGroup(Session.GetHabbo(), Name, Description, RoomId, Badge, Colour1, Colour2, out Group);
            if (Group == null)
            {
                Session.SendNotification(
                    "An error occured whilst trying to create this group.\n\nTry again. If you get this message more than once, report it to staff.\r\r");
                return;
            }

            Session.SendMessage(new PurchaseOkComposer());

            Room.Group = Group;

            Session.SendMessage(new NewGroupInfoComposer(RoomId, Group.Id));
        }
    }
}