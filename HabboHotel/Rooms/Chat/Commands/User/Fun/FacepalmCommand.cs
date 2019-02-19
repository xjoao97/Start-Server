#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class FacepalmCommand : IChatCommand
    {
        public string PermissionRequired => "command_facepalm";

        public string Parameters => "%username%";
        public string Description => "Não gosta de um usuário? Use este comando. HAHAHAHA";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Insira o nome do usuário que você deseja expulsar.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Você só pode usar este comando em seu quarto.");
                return;
            }
            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("Este usuário está offline ou não se encontra por aqui.");
                return;
            }

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null)
            {
                session.SendWhisper("Este usuário está offline ou não se encontra por aqui.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Eu realmente acho que você não iria querer fazer isso...!");
                return;
            }


            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            targetUser.MoveTo(room.GetGameMap().Model.DoorX, room.GetGameMap().Model.DoorY);
            thisUser.OnChat(thisUser.LastBubble,
                "*Bote suas mãos na cabeça," + targetClient.GetHabbo().Username + " e saia do meu quarto!*", true);

            targetUser.CanWalk = false;
            targetUser.AllowOverride = true;
        }
    }
}