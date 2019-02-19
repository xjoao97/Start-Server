#region

using System.Threading;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class JerkOffCommand : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "";

        public string Description => "Bater uma.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var roomUserByHabbo1 = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUserByHabbo1 == null)
                return;
            {
                {
                    if (session.GetHabbo().Gender == "m")
                    {
                        room.SendMessage(
                            new ChatComposer(roomUserByHabbo1.VirtualId,
                                "*Puxa as calças para baixo e joga o pênis para fora*", 0, 0));
                        Thread.Sleep(1000);
                        room.SendMessage(
                            new ChatComposer(roomUserByHabbo1.VirtualId, "*Bate uma como se não ouvesse amanhã*", 0, 0));
                        Thread.Sleep(3000);
                        room.SendMessage(
                            new ChatComposer(roomUserByHabbo1.VirtualId, "*Gozando pra TODO LADO!* Desculpa galera..", 0,
                                0));
                        Thread.Sleep(1000);
                        roomUserByHabbo1.ApplyEffect(9);
                        Thread.Sleep(3000);
                        roomUserByHabbo1.ApplyEffect(0);
                    }
                    else
                    {
                        room.SendMessage(
                            new ChatComposer(roomUserByHabbo1.VirtualId, "*Tira a calça e começa a esfregar a ppk*",
                                0, 0));
                        Thread.Sleep(2000);
                        room.SendMessage(new ChatComposer(roomUserByHabbo1.VirtualId, "*Awwwwwn hmmmmmmmmm*", 0, 0));
                        Thread.Sleep(2000);
                        room.SendMessage(
                            new ChatComposer(roomUserByHabbo1.VirtualId, "*Esguicho pra TODO LADO!* Desculpa galera..",
                                0, 0));
                        Thread.Sleep(1000);
                        roomUserByHabbo1.ApplyEffect(9);
                        Thread.Sleep(3000);
                        roomUserByHabbo1.ApplyEffect(0);
                    }
                }
            }
        }
    }
}