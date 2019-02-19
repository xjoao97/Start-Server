#region

using System;
using System.Threading;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class WelcomeCommand : IChatCommand
    {
        public string PermissionRequired => "command_sss";
        public string Parameters => "%usuário%";

        public string Description => "Deseje boas-vindas para um novo usuário!";

        public void Execute(GameClient Session, string[] Params)
        {
            if (Params.Length <= 1)
                return;
            var TargetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
                return;
            if (TargetClient.GetHabbo().AlreadyWelcome)
            {
                Session.SendWhisper("O usuário já recebeu as boas vindas!");
                return;
            }
            var Room = Session.GetHabbo().CurrentRoom;
            var TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
                return;

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
                return;
            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            if (!(Math.Abs(TargetUser.X - ThisUser.X) >= 9999999))
            {
                var thread = new Thread(() =>
                {
                    TargetClient.SendWhisper("Olá " + Params[1] + ", bem-vindo ao Hiddo Hotel!");
                    Thread.Sleep(2000); // 1
                    TargetClient.SendWhisper(
                        "Aqui temos um turbilhão de coisas para você, que tal começar pelos :comandos?");
                    Thread.Sleep(2000); // 1v
                    TargetClient.SendWhisper("A diversão reina, temos além de tudo, o famoso FAST FOOD! |");
                    Thread.Sleep(2000); // 1v
                    TargetClient.SendWhisper(
                        "Ouça suas músicas preferidas na nossa jukebox, basta adquirir um CD na loja.");
                    Thread.Sleep(2000); // 1
                    TargetClient.SendWhisper("É claro que você também irá querer chamar seus amigos, não é mesmo?");
                });
                thread.Start();
                TargetClient.GetHabbo().AlreadyWelcome = true;
            }
        }
    }
}