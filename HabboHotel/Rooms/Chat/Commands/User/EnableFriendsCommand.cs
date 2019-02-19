#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class EnableFriendsCommand : IChatCommand
    {
        public string PermissionRequired => "command_friends";

        public string Parameters => "";

        public string Description => "Ative os pedidos de amizade novamente";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().AllowFriendRequests = !session.GetHabbo().AllowFriendRequests;
            session.SendWhisper("Você " + (session.GetHabbo().AllowFriendRequests ? "habilitou" : "desabilitou") +
                                " pedidos de amizade.");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `block_newfriends` = '0' WHERE `id` = '" + session.GetHabbo().Id +
                                  "'");

                dbClient.RunQuery();
            }
        }
    }
}