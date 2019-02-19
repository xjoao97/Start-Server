#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class HugCommand : IChatCommand
    {
        public string Description => "Dê um soco na cara de alguém.";

        public string Parameters => "[ Usuario ]";

        public string PermissionRequired => "command_sss";

        public void Execute(GameClient session, string[] Params)
        {
//todo: rewrite this shit
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, digite o nome do usuário que você deseja socar!");
            }
            else
            {
                var clientByUsername = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                if (clientByUsername == null)
                {
                    session.SendWhisper("Sentimos muito, não encontramos este usuário.");
                }
                else
                {
                    var room = session.GetHabbo().CurrentRoom;

                    var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(clientByUsername.GetHabbo().Id);
                    if (roomUserByHabbo == null)
                    {
                        session.SendWhisper("Sentimos muito, não encontramos este usuário.");
                    }
                    else if (clientByUsername.GetHabbo().Username == session.GetHabbo().Username)
                    {
                        session.SendWhisper("Você é maluco? Ou masoquista? HAHA");
                        room.SendMessage(new ChatComposer(roomUserByHabbo.VirtualId, "Ajudem-me, sou masoquista!", 0, 34));
                    }
                    else if (roomUserByHabbo.TeleportEnabled)
                    {
                        session.SendWhisper("Sentimos muito, o usuário está com teleporte ativo.");
                    }
                    else
                    {
                        var user2 = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                        var targetId = room.GetRoomUserManager().GetRoomUserByHabbo(clientByUsername.GetHabbo().Id);
                        if (user2 != null)
                            if (Math.Abs(roomUserByHabbo.X - user2.X) < 2 &&
                                Math.Abs(roomUserByHabbo.Y - user2.Y) < 2)
                            {
                                room.SendMessage(new ChatComposer(user2.VirtualId,
                                    "*Dei um soco na sua cara, " + Params[1] + ".*", 0, 32));
                                room.SendMessage(new ChatComposer(targetId.VirtualId, "*AAAAAH, ISTO DOEU!*", 0, 22));

                                roomUserByHabbo.Statusses.Add("lay", "1.0 null");
                                roomUserByHabbo.Z -= 0.35;
                                roomUserByHabbo.isLying = true;
                                roomUserByHabbo.UpdateNeeded = true;
                                roomUserByHabbo.ApplyEffect(0x9d);
                            }
                            else
                            {
                                session.SendWhisper("Sentimos muito, " + Params[1] + " não está perto o suficiente!");
                            }
                    }
                }
            }
        }
    }
}