#region

using System;
using System.Threading;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class
        CutCommand : IChatCommand
    {
        public string PermissionRequired => "command_cut";

        public string Parameters => "%username%";
        public string Description => "Degole um usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (session.GetHabbo().LastCustomCommand + 10 >= OblivionServer.GetUnixTimestamp())
            {
                session.SendWhisper(Language.GetValue("command.wait"));
                return;
            }

            if (Params.Length == 1)
            {
                session.SendWhisper("Insira o nome do usuário!");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("Este usuário não está online ou não encontra-se no quarto, tente com outra pessoa.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (user == null)
            {
                session.SendWhisper("Este usuário não está online ou não encontra-se no quarto, tente com outra pessoa.");
                return;
            }
            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null || user == thisUser)
            {
                session.SendWhisper("Ora, você não quer se suicidar logo aqui, quer?");
                return;
            }

            if (Math.Abs(user.X - thisUser.X) < 2 && Math.Abs(user.Y - thisUser.Y) < 2)
            {
                thisUser.OnChat(6, "*Cortei a sua cabeça, " + targetClient.GetHabbo().Username + "HAHAHA*", false);
                room.SendMessage(new ChatComposer(user.VirtualId, "*MORTO*", 0, user.LastBubble));

                user.GetClient().SendWhisper("Você irá renascer em 3 segundos!");
                thisUser.ApplyEffect(117);
                user.ApplyEffect(93);
                targetClient.SendMessage(new FloodControlComposer(3));
                user.Frozen = true;
                var thrd = new Thread(delegate()
                {
                    Thread.Sleep(4000);
                    user.Frozen = false;
                    user.ApplyEffect(23);
                    Thread.Sleep(2000);
                    thisUser.ApplyEffect(0);
                    user.ApplyEffect(0);
                    user.OnChat(user.LastBubble, "*Renasci", false);
                });
                thrd.Start();
            }
            else
            {
                session.SendWhisper("Este usuário está muito longe, aproxime-se dele.");
            }
        }
    }
}