#region

using System;
using System.Data;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class ConvertCreditsCommand : IChatCommand
    {
        public string PermissionRequired => "command_convert_credits";

        public string Parameters => "";

        public string Description => "Converter os seus mobis em moedas.";

        public void Execute(GameClient session, string[] Params)
        {
            var totalValue = 0;

            try
            {
                DataTable table;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `items` WHERE `user_id` = '" + session.GetHabbo().Id +
                                      "' AND (`room_id`=  '0' OR `room_id` = '')");
                    table = dbClient.getTable();
                }

                if (table == null)
                {
                    session.SendWhisper("No momento você não tem itens no seu inventário!");
                    return;
                }

                foreach (DataRow row in table.Rows)
                {
                    var item = session.GetHabbo().GetInventoryComponent().GetItem(Convert.ToInt32(row[0]));
                    if (item == null)
                        continue;

                    if (!item.GetBaseItem().ItemName.StartsWith("CF_") &&
                        !item.GetBaseItem().ItemName.StartsWith("CFC_"))
                        continue;

                    if (item.RoomId > 0)
                        continue;

                    var split = item.GetBaseItem().ItemName.Split('_');
                    var value = int.Parse(split[1]);

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
                    }

                    session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id);

                    totalValue += value;

                    if (value > 0)
                    {
                        session.GetHabbo().Credits += value;
                        session.SendMessage(new CreditBalanceComposer(session.GetHabbo().Credits));
                    }
                }

                if (totalValue > 0)
                    session.SendNotification("Todos os créditos foram convertidos com sucesso!\r\r(Valor total: " +
                                             totalValue + " créditos!");
                else
                    session.SendNotification("Parece que você não tem nenhum item.");
            }
            catch
            {
                session.SendNotification("Opa, ocorreu um erro ao converter seus créditos!");
            }
        }
    }
}