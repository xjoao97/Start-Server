#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Moderation;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class MipCommand : IChatCommand
    {
        public string PermissionRequired => "command_mip";

        public string Parameters => "%username%";

        public string Description => "Banimento da máquina.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Insira o nome do usuário que você deseja banir a máquina.");
                return;
            }

            var habbo = OblivionServer.GetHabboByUsername(Params[1]);
            if (habbo == null)
            {
                session.SendWhisper("Não encontramos este usuário no nosso banco de dados.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") &&
                !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                session.SendWhisper("Você não pode banir este usuário.");
                return;
            }

            var ipAddress = string.Empty;
            var expire = OblivionServer.GetUnixTimestamp() + 78892200;
            var username = habbo.Username;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id +
                                  "' LIMIT 1");

                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + habbo.Id + "' LIMIT 1");
                ipAddress = dbClient.getString();
            }

            var reason = Params.Length >= 3 ? CommandManager.MergeParams(Params, 2) : "Razão não especificada.";

            if (!string.IsNullOrEmpty(ipAddress))
                OblivionServer.GetGame()
                    .GetModerationManager()
                    .BanUser(session.GetHabbo().Username, ModerationBanType.IP, ipAddress, reason, expire);
            OblivionServer.GetGame()
                .GetModerationManager()
                .BanUser(session.GetHabbo().Username, ModerationBanType.USERNAME, habbo.Username, reason, expire);

            if (!string.IsNullOrEmpty(habbo.MachineId))
                OblivionServer.GetGame()
                    .GetModerationManager()
                    .BanUser(session.GetHabbo().Username, ModerationBanType.MACHINE, habbo.MachineId, reason, expire);

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(username);
            targetClient?.Disconnect();
            session.SendWhisper("Sucesso, você baniu a máquina de '" + username + "' com a seguinte razão: '" +
                                reason + "'!");
        }
    }
}