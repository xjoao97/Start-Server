#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableWhispersCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_whispers";

        public string Parameters => "";

        public string Description => "Você pode habilitar ou desabilitar o recebimento de sussurros.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().ReceiveWhispers = !session.GetHabbo().ReceiveWhispers;
            session.SendWhisper("Você " + (session.GetHabbo().ReceiveWhispers ? "habilitou" : "desabilitou") +
                                " o recebimento de sussurros!");
        }
    }
}