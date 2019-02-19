#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class FollowCommand : IChatCommand
    {
        public string PermissionRequired => "command_follow";

        public string Parameters => "%username%";

        public string Description => "Você quer visitar um usuário especificado? Use este comando!";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Insira o nome do usuário que você deseja seguir.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("Um erro ocorreu ao tentarmos achar este usuário, talvez ele esteja offline.");
                return;
            }

            if (targetClient.GetHabbo().CurrentRoom == session.GetHabbo().CurrentRoom)
            {
                session.SendWhisper("Hey, abra seus olhos " + targetClient.GetHabbo().Username + " está aqui na sala.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Sadooooooooo!");
                return;
            }

            if (!targetClient.GetHabbo().InRoom)
            {
                session.SendWhisper("Este usuário não está em quarto algum!");
                return;
            }

            if (targetClient.GetHabbo().CurrentRoom.Access != RoomAccess.OPEN &&
                !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper(
                    "Opa, o quarto do usuário que você deseja seguir está trancado, você não pode segui-lo! :(");
                return;
            }

            session.GetHabbo().PrepareRoom(targetClient.GetHabbo().CurrentRoom.RoomId, "");
        }
    }
}