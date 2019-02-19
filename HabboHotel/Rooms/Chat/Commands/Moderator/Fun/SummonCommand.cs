#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class SummonCommand : IChatCommand
    {
        public string PermissionRequired => "command_summon";

        public string Parameters => "%username%";

        public string Description => "Puxe um usuário para perto.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira o nome do usuário que você deseja trazer.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("O usuário não está online.");
                return;
            }

            if (targetClient.GetHabbo() == null)
            {
                session.SendWhisper("O usuário não está online.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username ||
                targetClient.GetHabbo().CurrentRoomId == session.GetHabbo().CurrentRoomId)
            {
                session.SendWhisper("Get a life.");
                return;
            }

            targetClient.SendNotification("Você foi puxado por " + session.GetHabbo().Username + "!");
            if (!targetClient.GetHabbo().InRoom)
                targetClient.SendMessage(new RoomForwardComposer(session.GetHabbo().CurrentRoomId));
            else
                targetClient.GetHabbo().PrepareRoom(session.GetHabbo().CurrentRoomId, "");
        }
    }
}