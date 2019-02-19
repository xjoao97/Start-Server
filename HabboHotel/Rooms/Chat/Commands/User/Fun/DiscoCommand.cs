#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class DiscoCommand : IChatCommand
    {
        public string PermissionRequired => "command_disco";

        public string Parameters => "";

        public string Description => "Easter Egg, hehe.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (room == null || !room.CheckRights(session))
                return;

            room.DiscoMode = !room.DiscoMode;
        }
    }
}