/*#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class roomAcceptOffer : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "";

        public string Description => "Aceitar a oferta do quarto atual.";

        public void Execute(GameClient Session, string[] Params)
        {
            var CurrentRoom = Session.GetHabbo().CurrentRoom;
            var RoomOwner = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (RoomOwner.RoomOfferPending)
                if (RoomOwner.GetClient().GetHabbo().CurrentRoom.RoomData.RoomForSale)
                    if (RoomOwner.GetClient().GetHabbo().CurrentRoom.RoomData.OwnerId ==
                        RoomOwner.GetClient().GetHabbo().Id)
                    {
                        var OfferingUser = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(RoomOwner.RoomOfferUser);
                        OfferingUser.GetClient().SendWhisper("Este usuário aceitou a oferta, vendendo o quarto agora.");
                        NewRoomOwner(RoomOwner.GetClient().GetHabbo().CurrentRoom, OfferingUser, RoomOwner);
                        RoomOwner.RoomOfferPending = false;
                        RoomOwner.RoomOfferUser = 0;
                        RoomOwner.RoomOffer = "";
                    }
        }

        public void NewRoomOwner(Room RoomForSale, RoomUser BoughtRoomUser, RoomUser SoldRoomUser)
        {
            //Pre-Emptive Things
            using (var Adapter = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                Adapter.SetQuery("UPDATE rooms SET owner = @newowner WHERE id = @roomid");
                Adapter.AddParameter("newowner", BoughtRoomUser.HabboId);
                Adapter.AddParameter("roomid", RoomForSale.RoomData.Id);
                Adapter.RunQuery();

                Adapter.SetQuery("UPDATE items SET user_id = @newowner WHERE room_id = @roomid");
                Adapter.AddParameter("newowner", BoughtRoomUser.HabboId);
                Adapter.AddParameter("roomid", RoomForSale.RoomData.Id);
                Adapter.RunQuery();

                Adapter.SetQuery("DELETE FROM room_rights WHERE room_id = @roomid");
                Adapter.AddParameter("roomid", RoomForSale.RoomData.Id);
                Adapter.RunQuery();

                if (RoomForSale.Group != null)
                {
                    Adapter.SetQuery("SELECT id FROM groups WHERE room_id = @roomid");
                    Adapter.AddParameter("roomid", RoomForSale.RoomData.Id);

                    var GroupId = Adapter.getInteger();

                    if (GroupId > 0)
                    {
                        RoomForSale.Group.ClearRequests();

                        foreach (var MemberId in RoomForSale.Group.GetAllMembers)
                        {
                            RoomForSale.Group.DeleteMember(MemberId);

                            var MemberClient = OblivionServer.GetGame().GetClientManager().GetClientByUserID(MemberId);

                            if (MemberClient == null)
                                continue;

                            if (MemberClient.GetHabbo().GetStats().FavouriteGroupId == GroupId)
                                MemberClient.GetHabbo().GetStats().FavouriteGroupId = 0;
                        }

                        Adapter.RunQuery("DELETE FROM `groups` WHERE `id` = '" + RoomForSale.Group.Id + "'");
                        Adapter.RunQuery("DELETE FROM `group_memberships` WHERE `group_id` = '" + RoomForSale.Group.Id +
                                         "'");
                        Adapter.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + RoomForSale.Group.Id +
                                         "'");
                        Adapter.RunQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" +
                                         RoomForSale.Group.Id + "' LIMIT 1");
                        Adapter.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `groupid` = '" +
                                         RoomForSale.Group.Id + "' LIMIT 1");
                        Adapter.RunQuery("DELETE FROM `items_groups` WHERE `group_id` = '" + RoomForSale.Group.Id + "'");
                    }
                    OblivionServer.GetGame().GetGroupManager().DeleteGroup(RoomForSale.Group.Id);
                    RoomForSale.RoomData.Group = null;
                    RoomForSale.Group = null;
                }
            }
            //Change Room Owners
            RoomForSale.RoomData.OwnerId = BoughtRoomUser.HabboId;
            RoomForSale.RoomData.OwnerName = BoughtRoomUser.GetClient().GetHabbo().Username;

            //Change Item Owners
            foreach (var CurrentItem in RoomForSale.GetRoomItemHandler().GetWallAndFloor)
            {
                CurrentItem.UserID = BoughtRoomUser.HabboId;
                CurrentItem.Username = BoughtRoomUser.GetClient().GetHabbo().Username;
            }

            //Take Credits or Diamonds from User
            if (RoomForSale.RoomData.RoomSaleType == "c")
            {
                BoughtRoomUser.GetClient().GetHabbo().Credits -= RoomForSale.RoomData.RoomSaleCost;
                BoughtRoomUser.GetClient()
                    .SendMessage(new CreditBalanceComposer(BoughtRoomUser.GetClient().GetHabbo().Credits));
            }
            else if (RoomForSale.RoomData.RoomSaleType == "d")
            {
                BoughtRoomUser.GetClient().GetHabbo().Diamonds -= RoomForSale.RoomData.RoomSaleCost;
                BoughtRoomUser.GetClient()
                    .SendMessage(
                        new HabboActivityPointNotificationComposer(BoughtRoomUser.GetClient().GetHabbo().Diamonds, 0, 5));
            }


            //Give Credits or Diamonds to User
            if (RoomForSale.RoomData.RoomSaleType == "c")
            {
                SoldRoomUser.GetClient().GetHabbo().Credits += RoomForSale.RoomData.RoomSaleCost;
                SoldRoomUser.GetClient()
                    .SendMessage(new CreditBalanceComposer(SoldRoomUser.GetClient().GetHabbo().Credits));
            }
            else if (RoomForSale.RoomData.RoomSaleType == "d")
            {
                SoldRoomUser.GetClient().GetHabbo().Diamonds += RoomForSale.RoomData.RoomSaleCost;
                SoldRoomUser.GetClient()
                    .SendMessage(new HabboActivityPointNotificationComposer(
                        SoldRoomUser.GetClient().GetHabbo().Diamonds, 0, 5));
            }

            //Unsign Room
            RoomForSale.RoomData.RoomForSale = false;
            RoomForSale.RoomData.RoomSaleCost = 0;
            RoomForSale.RoomData.RoomSaleType = "";

            //Unload the Room
            Room R = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomForSale.Id, out R))
                return;
            var UsersToReturn = RoomForSale.GetRoomUserManager().GetRoomUsers().ToList();
            OblivionServer.GetGame().GetNavigator().Init();
            OblivionServer.GetGame().GetRoomManager().UnloadRoom(R, true);
            foreach (var User in UsersToReturn.Where(User => User?.GetClient() != null))
            {
                User.GetClient().SendMessage(new RoomForwardComposer(RoomForSale.Id));
                User.GetClient()
                    .SendNotification("<b> Alerta do quarto </b>\r\rO quarto foi adquirido por\r\r<b>" +
                                      BoughtRoomUser.GetClient().GetHabbo().Username + "</b>!");
            }
        }
    }
}*/