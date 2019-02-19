#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class UnloadCommand : IChatCommand
    {
        public string PermissionRequired => "command_unload";

        public string Parameters => "%id%";

        public string Description => "Recarregue o quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (session.GetHabbo().GetPermissions().HasRight("room_unload_any"))
            {
                Room r;
                if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(room.Id, out r))
                    return;

                OblivionServer.GetGame().GetRoomManager().UnloadRoom(r, true);
            }
            else
            {
                if (room.CheckRights(session, true))
                    OblivionServer.GetGame().GetRoomManager().UnloadRoom(room);
            }
        }
    }
}