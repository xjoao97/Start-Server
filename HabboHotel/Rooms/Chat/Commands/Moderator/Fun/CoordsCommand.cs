#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class CoordsCommand : IChatCommand
    {
        public string PermissionRequired => "command_coords";

        public string Parameters => "";

        public string Description => "Use para ver suas coordenadas no quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            session.SendNotification("X: " + thisUser.X + "\n - Y: " + thisUser.Y + "\n - Z: " + thisUser.Z +
                                     "\n - Rot: " + thisUser.RotBody + ", sqState: " +
                                     room.GetGameMap().GameMap[thisUser.X, thisUser.Y] + "\n\n - RoomID: " +
                                     session.GetHabbo().CurrentRoomId);
        }
    }
}