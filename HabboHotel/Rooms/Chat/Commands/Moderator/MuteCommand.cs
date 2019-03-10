#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class MuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute";

        public string Parameters => "%username% %time%";

        public string Description => "Mute um usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper(
                    "Por favor, digite o nome do usuário e o tempo em segundos que o mesmo ficará mudo (máximo 600).");
                return;
            }

            var habbo = OblivionServer.GetHabboByUsername(Params[1]);
            if (habbo == null)
            {
                session.SendWhisper("Não encontramos o usuário no banco de dados.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") &&
                !session.GetHabbo().GetPermissions().HasRight("mod_mute_any"))
            {
                session.SendWhisper("Você não pode silenciar este usuário.");
                return;
            }

            double time;
            if (double.TryParse(Params[2], out time))
            {
                if (time > 600 && !session.GetHabbo().GetPermissions().HasRight("mod_mute_limit_override"))
                    time = 600;

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("UPDATE `users` SET `time_muted` = '" + time + "' WHERE `id` = '" + habbo.Id +
                                      "' LIMIT 1");
                }

                if (habbo.GetClient() != null)
                {
                    habbo.TimeMuted = time;
                    habbo.GetClient()
                        .SendNotification("Você foi silenciado por um moderador por " + time + " segundos!");
                }

                session.SendWhisper("Você silenciou " + habbo.Username + " com sucesso, por " + time + " segundos.");
            }
            else
            {
                session.SendWhisper("Insira um valor válido (inteiro).");
            }
        }
    }
}