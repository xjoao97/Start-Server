using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class OnlineCommand : IChatCommand
    {
        public string PermissionRequired => "command_online";

        public string Parameters => "";

        public string Description => Language.GetValue("online.desc");

        public void Execute(GameClient Session, string[] Params)
        {
            var OnlineUsers = OblivionServer.GetGame().GetClientManager().Count;

            Session.SendWhisper(Language.GetValueWithVar("online.text", OnlineUsers.ToString()));
        }
    }
}