#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableDiagonalCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_diagonal";

        public string Parameters => "";

        public string Description => "Quer desativar os quadrados diagonais do seu quarto? Use esse comando!";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Hey, apenas o dono do quarto pode utilizar esse comando!");
                return;
            }

            room.GetGameMap().DiagonalEnabled = !room.GetGameMap().DiagonalEnabled;
            session.SendWhisper("Sucesso.");
        }
    }
}