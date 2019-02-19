#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class IgnoreWhispersCommand : IChatCommand
    {
        public string PermissionRequired => "command_ignore_whispers";

        public string Parameters => "";

        public string Description => "Permite que você ingore os sussurros dos usuários.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().IgnorePublicWhispers = !session.GetHabbo().IgnorePublicWhispers;
            session.SendWhisper("Sucesso!");
        }
    }
}