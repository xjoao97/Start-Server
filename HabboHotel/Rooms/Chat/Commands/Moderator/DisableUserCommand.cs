#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class DisableUserCommand : IChatCommand
    {
        public string PermissionRequired => "command_ban";

        public string Parameters => "";

        public string Description => "Desative um comando para um usuário";

        public void Execute(GameClient Session, string[] Params)
        {
            if (Params[1] == null || Params[2] == null)
                return;

            var cmd = Params[1];
            var habbo = OblivionServer.GetHabboByUsername(Params[2]);

            if (habbo == null)
            {
                Session.SendWhisper("Usuário não encotnrado");
                return;
            }
            if (!CommandManager.Commands.ContainsKey(cmd.ToLower()))
            {
                Session.SendWhisper("Comando não existente");
                return;
            }

            if (habbo.BlockedCommands.Contains(cmd))
            {
                Session.SendWhisper("Comando já bloqueado");
                return;
            }

            if (habbo.Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Você não pode bloquear seu próprio comando");
                return;
            }
            if (habbo.Rank >= Session.GetHabbo().Rank)
            {
                Session.SendWhisper("Você não pode bloquear um comando de um superior");
                return;
            }

            habbo.BlockedCommands.Add(cmd.ToLower());

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO user_blockcmd (user_id, command_name) VALUES (@user, @command)");
                dbClient.AddParameter("user", habbo.Id);
                dbClient.AddParameter("command", cmd);
                dbClient.RunQuery();
            }

            Session.SendWhisper("Comando desativado com sucesso");
        }
    }
}