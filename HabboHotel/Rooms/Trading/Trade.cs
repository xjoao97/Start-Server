#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Inventory.Trading;
using Oblivion.Core;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.HabboHotel.Rooms.Trading
{
    public class Trade
    {
        private readonly int oneId;
        private readonly int RoomId;
        private readonly int twoId;
        private int TradeStage;
        public TradeUser[] Users;

        public Trade(int UserOneId, int UserTwoId, int RoomId)
        {
            oneId = UserOneId;
            twoId = UserTwoId;

            Users = new TradeUser[2];
            Users[0] = new TradeUser(UserOneId, RoomId);
            Users[1] = new TradeUser(UserTwoId, RoomId);
            TradeStage = 1;
            this.RoomId = RoomId;

            foreach (var User in Users.ToList().Where(User => !User.GetRoomUser().Statusses.ContainsKey("trd")))
            {
                User.GetRoomUser().AddStatus("trd", "");
                User.GetRoomUser().UpdateNeeded = true;
            }

            SendMessageToUsers(new TradingStartComposer(UserOneId, UserTwoId));
        }

        public bool AllUsersAccepted => Users.Where(t => t != null).All(t => t.HasAccepted);

        public bool ContainsUser(int Id) => Users.Where(t => t != null).Any(t => t.UserId == Id);

        public TradeUser GetTradeUser(int Id) => Users.Where(t => t != null).FirstOrDefault(t => t.UserId == Id);

        public void OfferItem(int UserId, Item Item)
        {
            var User = GetTradeUser(UserId);

            if (User == null || Item == null || !Item.GetBaseItem().AllowTrade || User.HasAccepted ||
                TradeStage != 1)
                return;

            ClearAccepted();

            if (!User.OfferedItems.Contains(Item))
                User.OfferedItems.Add(Item);

            UpdateTradeWindow();
        }

        public void TakeBackItem(int UserId, Item Item)
        {
            var User = GetTradeUser(UserId);

            if (User == null || Item == null || User.HasAccepted || TradeStage != 1)
                return;

            ClearAccepted();

            User.OfferedItems.Remove(Item);
            UpdateTradeWindow();
        }

        public void Accept(int UserId)
        {
            var User = GetTradeUser(UserId);

            if (User == null || TradeStage != 1)
                return;

            User.HasAccepted = true;

            SendMessageToUsers(new TradingAcceptComposer(UserId, true));


            if (!AllUsersAccepted) return;
            SendMessageToUsers(new TradingCompleteComposer());
            TradeStage++;
            ClearAccepted();
        }

        public void Unaccept(int UserId)
        {
            var User = GetTradeUser(UserId);

            if (User == null || TradeStage != 1 || AllUsersAccepted)
                return;

            User.HasAccepted = false;

            SendMessageToUsers(new TradingAcceptComposer(UserId, false));
        }

        public void CompleteTrade(int UserId)
        {
            var User = GetTradeUser(UserId);

            if (User == null || TradeStage != 2)
                return;

            User.HasAccepted = true;

            SendMessageToUsers(new TradingConfirmedComposer(UserId, true));

            if (!AllUsersAccepted) return;
            TradeStage = 999;
            Finnito();
        }

        private void Finnito()
        {
            try
            {
                DeliverItems();
                CloseTradeClean();
            }
            catch (Exception e)
            {
                Logging.LogThreadException(e.ToString(), "Trade task");
            }
        }

        public void ClearAccepted()
        {
            foreach (var User in Users.ToList())
                User.HasAccepted = false;
        }

        public void UpdateTradeWindow() => SendMessageToUsers(new TradingUpdateComposer(this));

        public void DeliverItems()
        {
            // Deliver them
           
                // List items
                var ItemsOne = GetTradeUser(oneId).OfferedItems;
                var ItemsTwo = GetTradeUser(twoId).OfferedItems;

                var User1 = "";
                var User2 = "";

                // Verify they are still in user inventory
                foreach (var I in ItemsOne.ToList().Where(I => I != null))
                {
                    if (GetTradeUser(oneId).GetClient().GetHabbo().GetInventoryComponent().GetItem(I.Id) == null)
                    {
                        GetTradeUser(oneId)
                            .GetClient()
                            .SendNotification(OblivionServer.GetGame().GetLanguageLocale().TryGetValue("trade_failed"));
                        GetTradeUser(twoId)
                            .GetClient()
                            .SendNotification(OblivionServer.GetGame().GetLanguageLocale().TryGetValue("trade_failed"));
                        return;
                    }
                    User1 += I.Id + ";";
                }

                foreach (var I in ItemsTwo.ToList().Where(I => I != null))
                {
                    if (GetTradeUser(twoId).GetClient().GetHabbo().GetInventoryComponent().GetItem(I.Id) == null)
                    {
                        GetTradeUser(oneId)
                            .GetClient()
                            .SendNotification(OblivionServer.GetGame().GetLanguageLocale().TryGetValue("trade_failed"));
                        GetTradeUser(twoId)
                            .GetClient()
                            .SendNotification(OblivionServer.GetGame().GetLanguageLocale().TryGetValue("trade_failed"));

                        return;
                    }
                    User2 += I.Id + ";";
                }
            Task.Factory.StartNew(() =>
            {

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    foreach (var I in ItemsOne.ToList().Where(I => I != null))
                    {
                        GetTradeUser(oneId).GetClient().GetHabbo().GetInventoryComponent().RemoveItem(I.Id);
                        GetTradeUser(oneId).GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);

                        dbClient.SetQuery("UPDATE `items` SET `user_id` = @user WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("user", twoId);
                        dbClient.AddParameter("id", I.Id);
                        dbClient.RunQuery();

                        GetTradeUser(twoId)
                            .GetClient()
                            .GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(I.Id, I.BaseItem, I.ExtraData, I.GroupId, false, false, I.LimitedNo,
                                I.LimitedTot);
                        GetTradeUser(twoId).GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }

                    foreach (var I in ItemsTwo.ToList().Where(I => I != null))
                    {
                        GetTradeUser(twoId).GetClient().GetHabbo().GetInventoryComponent().RemoveItem(I.Id);

                        dbClient.SetQuery("UPDATE `items` SET `user_id` = @user WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("user", oneId);
                        dbClient.AddParameter("id", I.Id);
                        dbClient.RunQuery();

                        GetTradeUser(oneId)
                            .GetClient()
                            .GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(I.Id, I.BaseItem, I.ExtraData, I.GroupId, false, false, I.LimitedNo,
                                I.LimitedTot);
                    }
                }
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "INSERT INTO `logs_client_trade` VALUES(null, @1id, @2id, @1items, @2items, UNIX_TIMESTAMP())");
                    dbClient.AddParameter("1id", oneId);
                    dbClient.AddParameter("2id", twoId);
                    dbClient.AddParameter("1items", User1);
                    dbClient.AddParameter("2items", User2);
                    dbClient.RunQuery();
                }
            });
            // Update inventories
            GetTradeUser(oneId).GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
            GetTradeUser(twoId).GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
        }

        public void CloseTradeClean()
        {
            foreach (
                var User in
                Users.ToList()
                    .Where(User => User?.GetRoomUser() != null)
                    .Where(User => User.GetRoomUser().Statusses.ContainsKey("trd")))
            {
                User.GetRoomUser().RemoveStatus("trd");
                User.GetRoomUser().UpdateNeeded = true;
            }

            SendMessageToUsers(new TradingFinishComposer());
            GetRoom().ActiveTrades.Remove(this);
        }

        public void CloseTrade(int UserId)
        {
            foreach (
                var User in
                Users.ToList()
                    .Where(User => User?.GetRoomUser() != null)
                    .Where(User => User.GetRoomUser().Statusses.ContainsKey("trd")))
            {
                User.GetRoomUser().RemoveStatus("trd");
                User.GetRoomUser().UpdateNeeded = true;
            }

            SendMessageToUsers(new TradingClosedComposer(UserId));
        }

        public void SendMessageToUsers(ServerPacket Message)
        {
            foreach (var User in Users.ToList().Where(User => User?.GetClient() != null))
                User.GetClient().SendMessage(Message);
        }

        private Room GetRoom()
        {
            Room Room;
            return OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room) ? Room : null;
        }

        public int CountItems(TradeUser user) => user.OfferedItems.Count;
    }
}