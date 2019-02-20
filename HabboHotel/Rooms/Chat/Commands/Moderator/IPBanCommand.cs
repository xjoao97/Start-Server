#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Moderation;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class IpBanCommand : IChatCommand
    {
        public string PermissionRequired => "command_ip_ban";

        public string Parameters => "%username%";

        public string Description => "Banir o ip de algum usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite o nome do usuário que você deseja banir o IP.");
                return;
            }

            var habbo = OblivionServer.GetHabboByUsername(Params[1]);
            if (habbo == null)
            {
                session.SendWhisper("Ocorreu um erro ao encontrar este usuário na base de dados.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") &&
                !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                session.SendWhisper("Hey, você não pode banir este usuário.");
                return;
            }

            var ipAddress = string.Empty;
            var expire = OblivionServer.GetUnixTimestamp() + 78892200;
            var username = habbo.Username;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id +
                                  "' LIMIT 1");

                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + habbo.Id + "' LIMIT 1");
                ipAddress = dbClient.getString();
            }

            string reason = null;
            if (Params.Length >= 3)
                reason = CommandManager.MergeParams(Params, 2);
            else
                reason = "Razão não especificada";

            if (!string.IsNullOrEmpty(ipAddress))
                OblivionServer.GetGame()
                    .GetModerationManager()
                    .BanUser(session.GetHabbo().Username, ModerationBanType.IP, ipAddress, reason, expire);
            OblivionServer.GetGame()
                .GetModerationManager()
                .BanUser(session.GetHabbo().Username, ModerationBanType.USERNAME, habbo.Username, reason, expire);

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(username);
            if (targetClient != null)
                targetClient.Disconnect();


            session.SendWhisper("Sucesso, o IP da conta '" + username + "' foi banido, a razão: '" + reason +
                                "'!");
        }
    }
}