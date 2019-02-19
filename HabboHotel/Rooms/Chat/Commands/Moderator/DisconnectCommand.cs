#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class DisconnectCommand : IChatCommand
    {
        public string PermissionRequired => "command_disconnect";

        public string Parameters => "%username%";

        public string Description => "%USUARIO%";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Escreva o nome do usuário que você deseja desconectar.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("O usuário não está online.");
                return;
            }

            if (targetClient.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper("Você não pode desconectar este usuário.");
                return;
            }

            if (targetClient.GetHabbo().GetPermissions().HasRight("mod_tool") &&
                !session.GetHabbo().GetPermissions().HasRight("mod_disconnect_any"))
            {
                session.SendWhisper("Você não pode desconectá-lo.");
                return;
            }

            targetClient.GetConnection().Dispose();
        }
    }
}