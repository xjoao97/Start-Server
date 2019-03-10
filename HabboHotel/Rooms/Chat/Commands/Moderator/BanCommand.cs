#region

using System;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Moderation;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class BanCommand : IChatCommand
    {
        public string PermissionRequired => "command_ban";
        public string Parameters => "%username% %duração% %razão% ";

        public string Description => "Que tal banir um usuário que está atrapalhando o desenvolvimento do Hotel?";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite o nome do usuário que você gostaria de banir o IP e a conta.");
                return;
            }

            var habbo = OblivionServer.GetHabboByUsername(Params[1]);
            if (habbo == null)
            {
                session.SendWhisper("Ocorreu um erro ao encontrar esse usuário no banco de dados.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_soft_ban") &&
                !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                session.SendWhisper("Opa, você não pode banir este usuário.");
                return;
            }

            double expire = 0;
            var hours = Params[2];
            if (string.IsNullOrEmpty(hours) || hours == "perm")
                expire = OblivionServer.GetUnixTimestamp() + 78892200;
            else
                expire = OblivionServer.GetUnixTimestamp() + Convert.ToDouble(hours) * 3600;

            string reason = null;
            if (Params.Length >= 4)
                reason = CommandManager.MergeParams(Params, 3);
            else
                reason = "Razão não especificada.";

            var username = habbo.Username;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id +
                                  "' LIMIT 1");
            }

            OblivionServer.GetGame()
                .GetModerationManager()
                .BanUser(session.GetHabbo().Username, ModerationBanType.USERNAME, habbo.Username, reason, expire);

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(username);
            if (targetClient != null)
                targetClient.Disconnect();

            session.SendWhisper("Sucesso, você baniu '" + username + "' por " + hours +
                                " horas, e o motivo foi: '" + reason + "'!");
        }
    }
}