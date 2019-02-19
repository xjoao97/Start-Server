#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class roomBuyCommand : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "%offer%";

        public string Description
            => "Permite que você faça um acordo, solicitando um preço para o quarto [Ex. 1000c OU 200conchas]";

        public void Execute(GameClient Session, string[] Params)
        {
            var CurrentRoom = Session.GetHabbo().CurrentRoom;
            //Gets the current RoomUser
            var User = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            var RoomOwner = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(CurrentRoom.RoomData.OwnerId);
            //If it grabs an invalid user somehow, it returns
            if (User == null)
                return;

            if (RoomOwner == null)
            {
                User.GetClient().SendWhisper("Propietário do quarto não encontrado.", 3);
                return;
            }
            //Room is not for sale
            if (!CurrentRoom.RoomData.RoomForSale)
            {
                User.GetClient()
                    .SendWhisper("Você não pode possuir um quarto na qual o proprietário não está vendendo.", 3);
                return;
            }

            //If does not have any value or the length does not contain a number with letter
            if (Params.Length == 1 || Params[1].Length < 2)
            {
                Session.SendWhisper("Por favor, coloque uma oferta válida para o quarto [Ex. 145000c OR 75000conchas]",
                    3);
                return;
            }

            //Assigns the input to a variable
            var ActualInput = Params[1];

            //If they are the current room owner
            if (CurrentRoom.RoomData.OwnerId == User.HabboId)
            {
                Session.SendWhisper("Você não pode comprar seu próprio quarto, digite :sellroom 0c", 3);
                return;
            }

            var roomCostType = ActualInput.Substring(ActualInput.Length - 1);

            //Declares the variable to be assigned in the try statement if they entered a valid offer
            int roomCost;
            try
            {
                //Great! It's valid if it passes this try
                roomCost = int.Parse(ActualInput.Substring(0, ActualInput.Length - 1));
            }
            catch
            {
                //Nope, Invalid integer
                User.GetClient().SendWhisper("Você precisa inserir uma oferta de quarto válida.", 3);
                return;
            }

            //Is there offer out of bounds?
            if (roomCost < 1 || roomCost > 10000000)
            {
                User.GetClient().SendWhisper("Oferta Inválida, muito baixo ou muito alto.", 3);
                return;
            }

            //Start doing the checks
            if (roomCost == CurrentRoom.RoomData.RoomSaleCost && roomCostType == CurrentRoom.RoomData.RoomSaleType)
                if (roomCostType == "c")
                    if (User.GetClient().GetHabbo().Credits >= roomCost)
                        NewRoomOwner(CurrentRoom, User, RoomOwner);
                    else
                        User.GetClient().SendWhisper("Você não possuí moedas suficientes.", 3);
                else if (roomCostType == "d")
                    if (User.GetClient().GetHabbo().Diamonds >= roomCost)
                        NewRoomOwner(CurrentRoom, User, RoomOwner);
                    else
                        User.GetClient().SendWhisper("Você não possuí conchas suficientes.", 3);
                else
                    Session.SendWhisper(
                        "Você não deveria ter recebido este erro, entre em contato com a equipe do Hiddo.", 3);
            else
            {
                Session.SendWhisper($"O preço é {roomCost}{roomCostType}");
            }
        }

        public void NewRoomOwner(Room RoomForSale, RoomUser BoughtRoomUser, RoomUser SoldRoomUser)
        {
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

                            if (MemberClient?.GetHabbo().GetStats().FavouriteGroupId == GroupId)
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
            //RoomBuyCommand Updated By Hamada Zipto
            Room R;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomForSale.Id, out R))
                return;
            var UsersToReturn = RoomForSale.GetRoomUserManager().GetRoomUsers().ToList();
            OblivionServer.GetGame().GetNavigator().Init();
            OblivionServer.GetGame().GetRoomManager().UnloadRoom(R, true);
            foreach (var User in UsersToReturn.Where(User => User?.GetClient() != null))
            {
                User.GetClient().SendMessage(new RoomForwardComposer(RoomForSale.Id));
                User.GetClient()
                    .SendNotification("<b> Alerta do quarto </b>\r\rO quarto foi adquirido pelo\r\r<b>" +
                                      BoughtRoomUser.GetClient().GetHabbo().Username + "</b>!");
            }
        }

    }
}