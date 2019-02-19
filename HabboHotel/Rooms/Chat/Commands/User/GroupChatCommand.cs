#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class GroupChatCommand : IChatCommand
    {
        public string PermissionRequired => "command_info";

        public string Parameters => "diable/enable";

        public string Description => "alterar o estado do chat de seu grupo.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length < 2)
                return;

            var cmd = Params[1];

            var groups = OblivionServer.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id, true);
            var room = session.GetHabbo().CurrentRoom;
            foreach (var group in groups.ToList())
            {
                if (group.RoomId != room.RoomId) continue;
                var enabled = cmd == "enable";
                using (var dbclient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbclient.SetQuery("UPDATE groups SET has_chat = @status WHERE id = @groupid");
                    dbclient.AddParameter("status", enabled ? 1 : 0);
                    dbclient.AddParameter("groupid", group.Id);
                    dbclient.RunQuery();
                }
                foreach (var client in group.GetAllMembers.Select(member => OblivionServer.GetGame().GetClientManager().GetClientByUserID(member)))
                {
                    client?.SendMessage(enabled
                        ? new FriendListUpdateComposer(group)
                        : new FriendListUpdateComposer(-group.Id));
                }
                group.HasChat = enabled;
                session.SendWhisper(enabled ? "O chat do grupo foi ativado" : "O chat do grupo foi desativado");

                return;
            }
        }
    }
}