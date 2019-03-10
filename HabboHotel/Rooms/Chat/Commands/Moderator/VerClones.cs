#region

using System.Data;
using System.Text;
using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.Database.Interfaces;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class VerClonesCommand : IChatCommand
    {
        public string PermissionRequired => "command_verclones";

        public string Parameters => "%user%";

        public string Description => "Ver clones.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Porfavor ingrese el nombre del usuario a revisar.");
                return;
            }

            string str2;
            IQueryAdapter adapter;
            var username = Params[1];
            DataTable table;
            var builder = new StringBuilder();
            if (OblivionServer.GetGame().GetClientManager().GetClientByUsername(username) != null)
            {
                str2 = OblivionServer.GetGame().GetClientManager().GetClientByUsername(username).GetConnection().getIp();
                builder.AppendLine("Username :  " + username + " - Ip : " + str2);
                using (adapter = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    adapter.SetQuery("SELECT id,username FROM users WHERE ip_last = @ip OR ip_reg = @ip");
                    adapter.AddParameter("ip", str2);
                    table = adapter.GetTable();
                    builder.AppendLine("Clones encontrados: " + table.Rows.Count);
                    foreach (DataRow row in table.Rows)
                        builder.AppendLine(string.Concat("Id : ", row["id"], " - Username: ", row["username"]));
                }
                session.SendMessage(new MotdNotificationComposer(builder.ToString()));
            }
            else
            {
                using (adapter = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    adapter.SetQuery("SELECT ip_last FROM users WHERE username = @username");
                    adapter.AddParameter("username", username);
                    str2 = adapter.GetString();
                    builder.AppendLine("Username :  " + username + " - Ip : " + str2);
                    adapter.SetQuery("SELECT id,username FROM users WHERE ip_last = @ip OR ip_reg = @ip");
                    adapter.AddParameter("ip", str2);
                    table = adapter.GetTable();
                    builder.AppendLine("Clones encontrados: " + table.Rows.Count);
                    foreach (DataRow row in table.Rows)
                        builder.AppendLine(string.Concat("Id : ", row["id"], " - Username: ", row["username"]));
                }

                session.SendMessage(new MotdNotificationComposer(builder.ToString()));
            }
        }
    }
}