#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GlobalGiveCommand : IChatCommand
    {
        public string PermissionRequired => "command_global_currency";

        public string Parameters => "%type% %amount%";

        public string Description => "Envia créditos para todos os usuários.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, escolha o crédito (moedas, estrelas, conchas, asteroides)");
                return;
            }

            var updateVal = Params[1];
            switch (updateVal.ToLower())
            {
                case "coins":
                case "credits":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_coins"))
                    {
                        session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[2], out amount))
                    {
                        foreach (var client in OblivionServer.GetGame().GetClientManager().GetClients.ToList())
                        {
                            client.GetHabbo().Credits += amount;
                            client.SendMessage(new CreditBalanceComposer(client.GetHabbo().Credits));
                        }
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE users SET credits = credits + " + amount);
                        }
                        break;
                    }
                    session.SendWhisper("Quantidade inválida!");
                    break;
                }
                case "estrelas":

                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_pixels"))
                    {
                        session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[2], out amount))
                    {
                        foreach (var client in OblivionServer.GetGame().GetClientManager().GetClients.ToList())
                        {
                            client.GetHabbo().Duckets += amount;
                            client.SendMessage(new HabboActivityPointNotificationComposer(
                                client.GetHabbo().Duckets, amount));
                        }
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE users SET activity_points = activity_points + " + amount);
                        }
                        break;
                    }
                    session.SendWhisper("Oops, that appears to be an invalid amount!");
                    break;
                }
                case "conchas":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds"))
                    {
                        session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[2], out amount))
                    {
                        foreach (var client in OblivionServer.GetGame().GetClientManager().GetClients.ToList())
                        {
                            client.GetHabbo().Diamonds += amount;
                            client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().Diamonds,
                                amount,
                                5));
                        }
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE users SET vip_points = vip_points + " + amount);
                        }
                        break;
                    }
                    session.SendWhisper("Oops, that appears to be an invalid amount!");
                    break;
                }
                case "asteroides":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_gotw"))
                    {
                        session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[2], out amount))
                    {
                        foreach (var client in OblivionServer.GetGame().GetClientManager().GetClients.ToList())
                        {
                            client.GetHabbo().GOTWPoints = client.GetHabbo().GOTWPoints + amount;
                            client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().GOTWPoints,
                                amount, 103));
                        }
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE users SET gotw_points = gotw_points + " + amount);
                        }
                        break;
                    }
                    session.SendWhisper("Oops, that appears to be an invalid amount!");
                    break;
                }
            }
        }
    }
}