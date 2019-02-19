#region

using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableCommands : IChatCommand
    {
        public string PermissionRequired => "command_disable_diagonal";

        public string Parameters => "";

        public string Description => Language.GetValue("disableroomcmd.desc");

        public void Execute(GameClient Session, string[] Params)
        {
            var cmd = Params[1];
            var room = Session.GetHabbo().CurrentRoom;


            if (!room.CheckRights(Session, true))
            {
                Session.SendWhisper(Language.GetValue("user.notroomowner"));
                return;
            }

            if (room.RoomData.BlockedCommands.Contains(cmd))
            {
                Session.SendWhisper(Language.GetValue("command.alreadyadded"));
                return;
            }

            if (!CommandManager.Commands.ContainsKey(cmd.ToLower()))
            {
                Session.SendWhisper(Language.GetValue("command.notfound"));
                return;
            }


            room.RoomData.BlockedCommands.Add(cmd.ToLower());

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO room_blockcmd (room_id, command_name) VALUES (@room_id, @command)");
                dbClient.AddParameter("room_id", room.RoomId);
                dbClient.AddParameter("command", cmd.ToLower());
                dbClient.RunQuery();
            }

            Session.SendWhisper(Language.GetValue("command.blocked.success"));
        }
    }
}