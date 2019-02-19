#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class SuperPushCommand : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "%target%";

        public string Description => "Superpush another user. (Pushes them 3 squares away)";

        public void Execute(GameClient Session, string[] Params)
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to push.");
                return;
            }


            var TargetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("O usuário não está online.");
                return;
            }

            var TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
            {
                Session.SendWhisper(
                    "An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Come on, surely you don't want to push yourself!");
                return;
            }

            if (TargetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("No puedes empujar a este usuario.");
                return;
            }

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
                return;
            }

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            if (!(Math.Abs(TargetUser.X - ThisUser.X) >= 2 || Math.Abs(TargetUser.Y - ThisUser.Y) >= 2))
            {
                if (TargetUser.SetX - 1 == Room.GetGameMap().Model.DoorX ||
                    TargetUser.SetY - 1 == Room.GetGameMap().Model.DoorY)
                {
                    Session.SendWhisper("Please don't push that user out of the room :(!");
                    return;
                }

                if (TargetUser.SetX - 2 == Room.GetGameMap().Model.DoorX ||
                    TargetUser.SetY - 2 == Room.GetGameMap().Model.DoorY)
                {
                    Session.SendWhisper("Please don't push that user out of the room :(!");
                    return;
                }

                if (TargetUser.SetX - 3 == Room.GetGameMap().Model.DoorX ||
                    TargetUser.SetY - 3 == Room.GetGameMap().Model.DoorY)
                {
                    Session.SendWhisper("Please don't push that user out of the room :(!");
                    return;
                }


                switch (ThisUser.RotBody)
                {
                    case 4:
                        TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                        break;
                    case 0:
                        TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                        break;
                    case 6:
                        TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                        break;
                    case 2:
                        TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                        break;
                    case 3:
                        TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                        TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                        break;
                    case 1:
                        TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                        TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                        break;
                    case 7:
                        TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                        TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                        break;
                    case 5:
                        TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                        TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                        break;
                }

                Room.SendMessage(new ChatComposer(ThisUser.VirtualId, "*super pushes " + Params[1] + "*", 0,
                    ThisUser.LastBubble));
            }
            else
            {
                Session.SendWhisper("Oops, " + Params[1] + " is not close enough!");
            }
        }
    }
}