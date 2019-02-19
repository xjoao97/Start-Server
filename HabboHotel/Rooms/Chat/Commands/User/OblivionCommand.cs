#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class OblivionCommand : IChatCommand
    {
        public string PermissionRequired => "command_info";

        public string Parameters => "";

        public string Description => "Nosso emulador.";

        public void Execute(GameClient session, string[] Params)
        {
            var onlineUsers = OblivionServer.GetGame().GetClientManager().Count;
            session.SendMessage(new RoomNotificationComposer("Informações do servidor",
                "Oblivion é um projeto privado,\nnão está a venda.\nUsers: " + onlineUsers 
                , "Informations" , "", "", true));
        }
    }
}