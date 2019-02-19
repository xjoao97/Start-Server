#region

using System;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class TradeBanCommand : IChatCommand
    {
        public string PermissionRequired => "command_trade_ban";

        public string Parameters => "%target% %length%";

        public string Description => "Banir as trocas de um usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Banir as trocas de um usuário (min 1 day, max 365 dias).");
                return;
            }

            var habbo = OblivionServer.GetHabboByUsername(Params[1]);
            if (habbo == null)
            {
                session.SendWhisper("Ocorreu um erro ao procurar o usuário em nosso banco de dados");
                return;
            }

            if (Convert.ToDouble(Params[2]) == 0)
            {
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `user_id` = '" + habbo.Id +
                                      "' LIMIT 1");
                }

                if (habbo.GetClient() != null)
                {
                    habbo.TradingLockExpiry = 0;
                    habbo.GetClient().SendNotification("Seu banimento de trocas foi cancelado.");
                }

                session.SendWhisper("Você removeu com sucesso o banimento das trocas de: " + habbo.Username + "");
                return;
            }

            double days;
            if (double.TryParse(Params[2], out days))
            {
                if (days < 1)
                    days = 1;

                if (days > 365)
                    days = 365;

                var length = OblivionServer.GetUnixTimestamp() + days * 86400;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '" + length +
                                      "', `trading_locks_count` = `trading_locks_count` + '1' WHERE `user_id` = '" +
                                      habbo.Id + "' LIMIT 1");
                }

                if (habbo.GetClient() != null)
                {
                    habbo.TradingLockExpiry = length;
                    habbo.GetClient().SendNotification("Suas trocas foram banidas por: " + days + " dias(s)!");
                }

                session.SendWhisper("Você baniu as trocas de: " + habbo.Username + " por " + days + " dia(s).");
            }
            else
            {
                session.SendWhisper("Insira um valor válido.");
            }
        }
    }
}