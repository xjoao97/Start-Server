#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class MassGiveCommand : IChatCommand
    {
        public string PermissionRequired => "command_mass_give";

        public string Parameters => "%type% %amount%";

        public string Description => "";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Escolha o tipo de crédito (moedas, estrela, conchas, asteroides)");
                return;
            }

            var updateVal = Params[1];
            switch (updateVal.ToLower())
            {
                case "moedas":

                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_coins"))
                    {
                        session.SendWhisper("Você não tem permissão para utilzar este comando!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[2], out amount))
                    {
                        foreach (
                            var client in
                            OblivionServer.GetGame()
                                .GetClientManager()
                                .GetClients.ToList()
                                .Where(
                                    client =>
                                        client?.GetHabbo() != null &&
                                        client.GetHabbo().Username != session.GetHabbo().Username))
                        {
                            client.GetHabbo().Credits = client.GetHabbo().Credits += amount;
                            client.SendMessage(new CreditBalanceComposer(client.GetHabbo().Credits));

                            if (client.GetHabbo().Id != session.GetHabbo().Id)
                                client.SendNotification(session.GetHabbo().Username + " lhe deu " + amount +
                                                        " moeda(s)!");
                            session.SendWhisper("Sucesso, você deu " + amount + " moeda(s) para " +
                                                client.GetHabbo().Username + "!");
                        }

                        break;
                    }
                    session.SendWhisper("Quantidade inválida");
                    break;
                }

                case "estrelas":
                
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_pixels"))
                    {
                        session.SendWhisper("Você não tem permissão para utilzar este comando!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[2], out amount))
                    {
                        foreach (
                            var client in
                            OblivionServer.GetGame()
                                .GetClientManager()
                                .GetClients.ToList()
                                .Where(
                                    client =>
                                        client?.GetHabbo() != null &&
                                        client.GetHabbo().Username != session.GetHabbo().Username))
                        {
                            client.GetHabbo().Duckets += amount;
                            client.SendMessage(new HabboActivityPointNotificationComposer(
                                client.GetHabbo().Duckets, amount));

                            if (client.GetHabbo().Id != session.GetHabbo().Id)
                                client.SendNotification(session.GetHabbo().Username + " lhe deu " + amount +
                                                        " Ducket(s)!");
                            session.SendWhisper("Você enviou " + amount + " Ducket(s) para " +
                                                client.GetHabbo().Username + "!");
                        }
                        break;
                    }
                    session.SendWhisper("Quantidade inválida");
                    break;
                }

                case "conchas":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds"))
                    {
                        session.SendWhisper("Você não tem permissões para utilizar este comando.");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[2], out amount))
                    {
                        foreach (
                            var client in
                            OblivionServer.GetGame()
                                .GetClientManager()
                                .GetClients.ToList()
                                .Where(
                                    client =>
                                        client?.GetHabbo() != null &&
                                        client.GetHabbo().Username != session.GetHabbo().Username))
                        {
                            client.GetHabbo().Diamonds += amount;
                            client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().Diamonds,
                                amount,
                                5));

                            if (client.GetHabbo().Id != session.GetHabbo().Id)
                                client.SendNotification(session.GetHabbo().Username + " lhe deu " + amount +
                                                        " Concha(s)!");
                            session.SendWhisper("Você enviou " + amount + " Concha(s) para " +
                                                client.GetHabbo().Username + "!");
                        }

                        break;
                    }
                    session.SendWhisper("Quantidade inválida!");
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
                        foreach (
                            var client in
                            OblivionServer.GetGame()
                                .GetClientManager()
                                .GetClients.ToList()
                                .Where(
                                    client =>
                                        client?.GetHabbo() != null &&
                                        client.GetHabbo().Username != session.GetHabbo().Username))
                        {
                            client.GetHabbo().GOTWPoints = client.GetHabbo().GOTWPoints + amount;
                            client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().GOTWPoints,
                                amount, 103));

                            if (client.GetHabbo().Id != session.GetHabbo().Id)
                                client.SendNotification(session.GetHabbo().Username + " te deu " + amount +
                                                        " asteroide(s)!");
                            session.SendWhisper("Enviado com sucesso " + amount + " asteroides(s) para " +
                                                client.GetHabbo().Username + "!");
                        }
                        break;
                    }
                    session.SendWhisper("Oops, that appears to be an invalid amount!");
                    break;
                }
                default:
                    session.SendWhisper("'" + updateVal + "' moeda inválida!");
                    break;
            }
        }
    }
}