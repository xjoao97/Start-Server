#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DndCommand : IChatCommand
    {
        public string PermissionRequired => "command_dnd";

        public string Parameters => "";

        public string Description => "Não quer receber mensagens em seu console? Use esse comando.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().AllowConsoleMessages = !session.GetHabbo().AllowConsoleMessages;
            session.SendWhisper("Você " + (session.GetHabbo().AllowConsoleMessages ? "habilitou" : "desabilitou") +
                                " o recebimento de mensagens em seu console.");
        }
    }
}