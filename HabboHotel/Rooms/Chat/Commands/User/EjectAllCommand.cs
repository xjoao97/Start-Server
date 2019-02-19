/*#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class EjectAllCommand : IChatCommand
    {
        public string PermissionRequired => "command_ejectall";

        public string Parameters => "";

        public string Description => "Remova todos os itens do seu quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            if (session.GetHabbo().Id == room.OwnerId)
            {
                //Let us check anyway.
                if (!room.CheckRights(session, true))
                    return;

                foreach (var item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
                {
                    if ((item == null) || (item.UserID == session.GetHabbo().Id))
                        continue;

                    var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUserID(item.UserID);
                    if (targetClient?.GetHabbo() != null)
                    {
                        room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                        targetClient.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo,
                                item.LimitedTot);
                        targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                    else
                    {
                        room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id +
                                              "' LIMIT 1");
                        }
                    }
                }
            }
            else
            {
                foreach (var item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
                {
                    if ((item == null) || (item.UserID != session.GetHabbo().Id))
                        continue;

                    var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUserID(item.UserID);
                    if (targetClient?.GetHabbo() != null)
                    {
                        room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                        targetClient.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo,
                                item.LimitedTot);
                        targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                    else
                    {
                        room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id +
                                              "' LIMIT 1");
                        }
                    }
                }
            }
        }
    }
}
*/

