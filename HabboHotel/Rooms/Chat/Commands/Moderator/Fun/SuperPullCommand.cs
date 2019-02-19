﻿#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class SuperPullCommand : IChatCommand
    {
        public string PermissionRequired => "command_super_pull";

        public string Parameters => "%username%";

        public string Description => "Pull another user to you, with no limits!";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to super pull.");
                return;
            }


            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("O usuário não está online.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null)
            {
                session.SendWhisper(
                    "An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Come on, surely you don't want to push yourself!");
                return;
            }

            if (targetUser.TeleportEnabled)
            {
                session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
                return;
            }

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            if (thisUser.SetX - 1 == room.GetGameMap().Model.DoorX)
            {
                session.SendWhisper("Please don't pull that user out of the room :(!");
                return;
            }

            if (thisUser.RotBody % 2 != 0)
                thisUser.RotBody--;
            if (thisUser.RotBody == 0)
                targetUser.MoveTo(thisUser.X, thisUser.Y - 1);
            else if (thisUser.RotBody == 2)
                targetUser.MoveTo(thisUser.X + 1, thisUser.Y);
            else if (thisUser.RotBody == 4)
                targetUser.MoveTo(thisUser.X, thisUser.Y + 1);
            else if (thisUser.RotBody == 6)
                targetUser.MoveTo(thisUser.X - 1, thisUser.Y);

            room.SendMessage(new ChatComposer(thisUser.VirtualId, "*super pulls " + Params[1] + " to them*", 0,
                thisUser.LastBubble));
        }
    }
}