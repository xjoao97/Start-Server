#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableFriendsCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_friends";

        public string Parameters => "";

        public string Description => "Desative as solicitações de amizade.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().AllowFriendRequests = !session.GetHabbo().AllowFriendRequests;
            session.SendWhisper("Você " + (session.GetHabbo().AllowFriendRequests ? "ativou" : "desativou") +
                                " os pedidos de amizade.");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `block_newfriends` = '1' WHERE `id` = '" + session.GetHabbo().Id +
                                  "'");
                dbClient.RunQuery();
            }
        }
    }
}