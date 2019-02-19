#region

using System;
using System.Threading;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    public class SexCommand : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "%username%";

        public string Description => Language.GetValue("sex.desc");
        public void Execute(GameClient session, string[] Params) => Command(session, Params[1]);

        public void Command(GameClient Session, string ToFuck)
        {
            if (Session.GetHabbo().LastCustomCommand + 300 >= OblivionServer.GetUnixTimestamp())
            {
                Session.SendWhisper(Language.GetValue("command.wait"));
                return;
            }

            if (ToFuck.Length <= 0)
            {
                Session.SendWhisper(Language.GetValue("user.notinroom"));
                return;
            }
            var Room = Session.GetHabbo().CurrentRoom;
            var Fucker = Session.GetHabbo();
            var Fucked = OblivionServer.GetHabboByUsername(ToFuck);
            if (Fucker == null || Fucked == null || Fucker == Fucked) return;

            var FuckerRoom = Room.GetRoomUserManager().GetRoomUserByHabbo(Fucker.Id);
            var FuckedRoom = Room.GetRoomUserManager().GetRoomUserByHabbo(Fucked.Id);

            if (Fucker.CurrentRoomId != Fucked.CurrentRoomId || Math.Abs(checked(FuckerRoom.X - FuckedRoom.X)) > 2)
            {
                Session.SendWhisper(Language.GetValue("user.tooclose"));
                return;
            }


            if (string.IsNullOrEmpty(Fucker.sexWith) && Fucked.Username != Fucker.sexWith &&
                Fucker.Username != Fucked.sexWith)
            {
                Fucker.sexWith = Fucked.Username;
                Fucked.GetClient()
                    .SendNotification(Fucker.Username + " Solicitou ter sexo com você. Para ter relações sexuais com " +
                                      Fucker.Username + ", digite \":sex " + Fucker.Username + "\"");
                Session.SendNotification(Fucked.Username +
                                         " Foi enviado o seu pedido de sexo. Se ele aceitar, você será capaz de transar.");
                return;
            }

            if (Fucker.Gender == "m")
            {
                var thread = new Thread(() => //new thread to no lag :V
                    {
                        Room.SendMessage(new ChatComposer(FuckerRoom.VirtualId,
                            "*Agarrando " + ToFuck + " Por trás, e começar a transar*", 0, 0));
                        Thread.Sleep(2000);
                        FuckerRoom.ApplyEffect(9);
                        FuckedRoom.ApplyEffect(507);
                        Room.SendMessage(
                            new ChatComposer(FuckedRoom.VirtualId,
                                "*Sobe em cima, preparando para meter em " + ToFuck + "*", 0, 0));
                        Thread.Sleep(2000);
                        Room.SendMessage(
                            new ChatComposer(FuckerRoom.VirtualId,
                                "*Começa a meter forte " + ToFuck + " dando aquele tesão*", 0, 0));
                        Room.SendMessage(
                            new ChatComposer(FuckedRoom.VirtualId, "Awwwwwn hmmmmmmmmmm*", 0, 0));
                        Thread.Sleep(2000);
                        Room.SendMessage(
                            new ChatComposer(FuckerRoom.VirtualId, "*Sai um pouco, quase gozando*",
                                0, 0));
                        Room.SendMessage(
                            new ChatComposer(FuckedRoom.VirtualId,
                                "*Tentando conter esse orgasmo delicioso " + Fucker.Username +
                                "*", 0, 0));
                        Thread.Sleep(2000);
                        Room.SendMessage(
                            new ChatComposer(FuckerRoom.VirtualId,
                                "*Gozando dentro " + ToFuck + "*", 0, 0));
                        Thread.Sleep(2000);
                        FuckerRoom.ApplyEffect(0);
                        FuckedRoom.ApplyEffect(0);
                    }
                );
                thread.Start();
            }
            else
            {
                var thread = new Thread(() =>
                {
                    Room.SendMessage(
                        new ChatComposer(FuckedRoom.VirtualId,
                            "*Esfregando o pênis de " + Fucker.Username + "*", 0, 0));
                    Thread.Sleep(1000);
                    FuckerRoom.ApplyEffect(9);
                    FuckedRoom.ApplyEffect(507);
                    Room.SendMessage(
                        new ChatComposer(FuckerRoom.VirtualId, "*Tira a calça*", 0, 0));
                    Thread.Sleep(2000);
                    Room.SendMessage(
                        new ChatComposer(FuckedRoom.VirtualId,
                            "*Coloca com cuidado " + Fucker.Username +
                            " bucetinha apertadinha*", 0, 0));
                    Room.SendMessage(
                        new ChatComposer(FuckerRoom.VirtualId,
                            "*Mmmmmm* Isos é tão bom...Hehe", 0, 0));
                    Thread.Sleep(2000);
                    Room.SendMessage(
                        new ChatComposer(FuckerRoom.VirtualId, "*mm, mm, mm, mm, mmmmmm!*", 0,
                            0));
                    Thread.Sleep(2000);
                    Room.SendMessage(
                        new ChatComposer(FuckedRoom.VirtualId,
                            "*Afastando " + Fucker.Username +
                            " esguichando pra todo lado*", 0, 0));
                    Room.SendMessage(
                        new ChatComposer(FuckerRoom.VirtualId,
                            "*Mordendo os lábios* Isso foi demais!", 0, 0));
                    Thread.Sleep(1000);
                    FuckerRoom.ApplyEffect(0);
                    FuckedRoom.ApplyEffect(0);
                });
                thread.Start();
            }
            Fucker.sexWith = null;
            Fucked.sexWith = null;
            Fucked.LastCustomCommand = OblivionServer.GetUnixTimestamp();
            Fucker.LastCustomCommand = OblivionServer.GetUnixTimestamp();
        }
    }
}