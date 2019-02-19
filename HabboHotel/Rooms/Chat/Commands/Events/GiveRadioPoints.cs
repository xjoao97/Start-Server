﻿using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class GiveRadioPoints : IChatCommand
    {
        public string PermissionRequired => "command_radiopoins";

        public string Parameters => "%usuário%";

        public string Description => "";

        public void Execute(GameClient session, string[] Params)
        {
            var target = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (target == null)
            {
                session.SendWhisper("Opa, não coneseguimos encontrar esse usuário!");
                return;
            }

            var userId = target.GetHabbo().Id;
            if (userId == session.GetHabbo().Id)
                return;

            const int amount = 5;
            target.GetHabbo().GOTWPoints += amount;
            target.GetHabbo().EventPoint += 1;
            target.SendMessage(new HabboActivityPointNotificationComposer(target.GetHabbo().GOTWPoints, amount,
                103));

            
            target.SendWhisper("Você recebeu " + amount +
                               " <b>asteroides</b>, parabéns!");
            OblivionServer.GetGame()
                .GetClientManager().SendMessage(new RoomNotificationComposer("",
                    "O usuário " + target.GetHabbo().Username + " ganhou o evento da rádio, parabéns!",
                    "fig/" + target.GetHabbo().Look,
                    session.GetHabbo().Username, "" + "event:friendbar/user/" + session.GetHabbo().Username, true));

            session.SendWhisper("Sucesso, você enviou o prêmio para " +
                                target.GetHabbo().Username + "!");
        }
    }
}