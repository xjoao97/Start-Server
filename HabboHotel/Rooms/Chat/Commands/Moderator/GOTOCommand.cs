#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GotoCommand : IChatCommand
    {
        public string PermissionRequired => "command_goto";

        public string Parameters => "%room_id%";

        public string Description => "";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Você deve especificar o ID do quarto!");
                return;
            }

            int roomId;

            if (!int.TryParse(Params[1], out roomId))
            {
                session.SendWhisper("Insira um ID válido!");
                return;
            }

            if (roomId == session.GetHabbo().CurrentRoomId)
                return;

            var rRoom = OblivionServer.GetGame().GetRoomManager().LoadRoom(roomId);
            if (rRoom == null)
                session.SendWhisper("Este quarto não existe!");
            else
                session.GetHabbo().PrepareRoom(rRoom.Id, "");
        }
    }
}