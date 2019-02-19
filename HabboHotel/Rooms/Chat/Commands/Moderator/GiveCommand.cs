#region

using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GiveCommand : IChatCommand
    {
        public string PermissionRequired => "command_give";

        public string Parameters => "%usuário% %moeda% %quantidade%";

        public string Description => "";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite a moeda que você deseja enviar! (creditos, estrelas, conchas, asteroides)");
                return;
            }

            var target = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (target == null)
            {
                session.SendWhisper("Opa, não coneseguimos encontrar esse usuário!");
                return;
            }

            var updateVal = Params[2];
            switch (updateVal.ToLower())
            {
                case "creditos":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_coins"))
                    {
                        session.SendWhisper("Opa, parece que você não tem as permissões para usar este comando!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[3], out amount))
                    {
                        target.GetHabbo().Credits = target.GetHabbo().Credits += amount;
                        target.GetHabbo().UpdateCreditsBalance(target);

                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " lhe deu " + amount +
                                                    " crédito(s)!");
                        session.SendWhisper("Sucesso, você enviou " + amount + " créditos(s) para " +
                                            target.GetHabbo().Username + "!");
                        break;
                    }
                    session.SendWhisper("Quantidade inválida.");
                    break;
                }

                case "estrelas":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_pixels"))
                    {
                        session.SendWhisper("Opa, parece que você não tem as permissões para usar este comando!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[3], out amount))
                    {
                        target.GetHabbo().Duckets += amount;
                        target.SendMessage(new HabboActivityPointNotificationComposer(target.GetHabbo().Duckets, amount));

                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " lhe deu " + amount +
                                                    " Estrela(s) do mar!");
                        session.SendWhisper("Sucesso, você enviou " + amount + " Estrela(s) do mar para " +
                                            target.GetHabbo().Username + "!");
                        break;
                    }
                    session.SendWhisper("Quantidade inválida!");
                    break;
                }

                case "conchas":

                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds"))
                    {
                        session.SendWhisper("Opa, parece que você não tem as permissões para usar este comando!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[3], out amount))
                    {
                        target.GetHabbo().Diamonds += amount;
                        target.SendMessage(new HabboActivityPointNotificationComposer(target.GetHabbo().Diamonds, amount,
                            5));

                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " lhe deu " + amount +
                                                    " Concha(s)!");
                        session.SendWhisper("Sucesso, você enviou " + amount + " concha(s) para " +
                                            target.GetHabbo().Username + "!");
                        break;
                    }
                    session.SendWhisper("Quantidade inválida.");
                    break;
                }

                case "asteroides":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_give_gotw"))
                    {
                        session.SendWhisper("Opa, você não tem permissão para isto!");
                        break;
                    }
                    int amount;
                    if (int.TryParse(Params[3], out amount))
                    {
                        target.GetHabbo().GOTWPoints = target.GetHabbo().GOTWPoints + amount;
                        target.SendMessage(new HabboActivityPointNotificationComposer(target.GetHabbo().GOTWPoints,
                            amount, 103));

                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " lhe deu " + amount +
                                                    " asteroides!");
                        session.SendWhisper("Você enviou " + amount + " asteroides para " +
                                            target.GetHabbo().Username + "!");
                        break;
                    }
                    session.SendWhisper("Opa, quantidade inválida!");
                    break;
                }
                default:
                    session.SendWhisper("'" + updateVal + "' isto não é uma moeda válida!");
                    break;
            }
        }
    }
}