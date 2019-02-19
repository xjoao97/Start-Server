#region

using System;
using Oblivion.HabboHotel.GameClients;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class InfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_info";

        public string Parameters => "";

        public string Description => "Convenhamos, todos nós adoramos ver estas informações.";

        public void Execute(GameClient session, string[] Params)
        {
            var uptime = DateTime.Now - OblivionServer.ServerStarted;
            var onlineUsers = OblivionServer.GetGame().GetClientManager().Count;
            var roomCount = OblivionServer.GetGame().GetRoomManager().Count;

            session.SendMessage(new RoomNotificationComposer("Informações do servidor",
                "<font color=\"#0489B1\" size=\"18\">[¶] Start Server:</font>\n\n" +
                "<b>Agradecimientos a:</b>\n" +
                "\t- Nillus\n" +
                "\t- Sledmore\n" +
                "\t- Meth0d\n" +
                "\t- Martinmine\n" +
                "\t- Claudinho Sant0ro\n" +
                "<b>Start Server Developers:</b>\n" +
                "\t- Droppy\n" +
                "\t- Dark\n" +
                "\t- OldFl4sh\n" +
                "\t- Xjoao\n" +
                "<b>Informação Atual</b>:\n" +
                "\t- Usuários Online: " + onlineUsers + "\n" +
                "\t- Recorde de Usuários: " + Game.SessionUserRecord + "\n" +
                "\t- Salas Carregadas: " + roomCount + "\n" +
                "\t- Tempo: " + uptime.Days + " dia(s), " + uptime.Hours + " hora(s) e " + uptime.Minutes +
                " minuto(s).\n\n", "", ""));
        }
    }
}