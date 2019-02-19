#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class GroupAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_info";

        public string Parameters => "diable/enable";

        public string Description => "Deseja sair do chat de grupo?";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length < 2)
                return;

            var cmd = Params[1];

            var groups = OblivionServer.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id);
            foreach (var group in groups.ToList())
            {
                var room = session.GetHabbo().CurrentRoom;

                if (group.RoomId != room.RoomId) continue;
                var enabled = cmd == "enable";

                if (enabled && group.ChatUsers.Contains(session.GetHabbo().Id))
                    return;

                using (var dbclient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbclient.SetQuery(
                        "UPDATE group_memberships SET has_chat = @status WHERE group_id = @groupid AND user_id = @user");
                    dbclient.AddParameter("status", enabled ? 1 : 0);
                    dbclient.AddParameter("groupid", group.Id);
                    dbclient.AddParameter("user", session.GetHabbo().Id);
                    dbclient.RunQuery();
                }

                if (group.ChatUsers.Contains(session.GetHabbo().Id))
                    group.ChatUsers.Remove(session.GetHabbo().Id);
                else
                    group.ChatUsers.Add(session.GetHabbo().Id);


                session.SendWhisper(enabled ? "O chat do grupo foi ativado" : "O chat do grupo foi desativado");
                session.SendMessage(enabled
                    ? new FriendListUpdateComposer(group)
                    : new FriendListUpdateComposer(-group.Id));

                return;
            }
        }
    }
}