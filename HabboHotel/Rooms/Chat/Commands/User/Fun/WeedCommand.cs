#region

using System;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class WeedCommand : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "";

        public string Description => "Fuma uma maconha mano, é de graça.";

        public void Execute(GameClient session, string[] Params)
        {
            if (session.GetHabbo().LastCustomCommand + 10 >= OblivionServer.GetUnixTimestamp())
            {
                session.SendWhisper(Language.GetValue("command.wait"));
                return;
            }
            var room = session.GetHabbo().CurrentRoom;
            var user = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;
            Task.Run(async () =>
            {
                user.ApplyEffect(551);
                user.GetClient()?.SendWhisper("Wooooow!");
                await Task.Delay(1000);
                user.OnChat(6, "*Puxa o brown e acende*", false);
                await Task.Delay(2000);
                user.OnChat(6, "*Dando uns tapa na verdinha*", false);
                await Task.Delay(4000);
                switch (new Random().Next(1, 4))
                {
                    case 1:
                        user.OnChat(6, "Sai daê gnomo, desce da nuvem doido?", false);
                        break;
                    case 2:
                        user.OnChat(6, "Tem um dinossauro preso na coleira", false);
                        break;
                    default:
                        user.OnChat(6, "KATRINA? MEU PAI? O MEU PAI?", false);
                        break;
                }
                await Task.Delay(3000);
                user.OnChat(6, "HAHAAHHAHAHAHAAHAHAHHAHAHAHA ETÁ PORRA", false);
                await Task.Delay(2000);
                user.OnChat(6, "*Fuma o restinho e manda fora*", false);
                user.ApplyEffect(0);
            });
        }
    }
}