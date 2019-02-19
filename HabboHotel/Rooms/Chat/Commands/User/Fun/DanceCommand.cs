#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class DanceCommand : IChatCommand
    {
        public string PermissionRequired => "command_dance";

        public string Parameters => "%DanceId%";

        public string Description => "Preguiçoso demais para escolher uma dança?!";

        public void Execute(GameClient session, string[] Params)
        {
            var thisUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira o ID da dança.");
                return;
            }

            int danceId;
            if (int.TryParse(Params[1], out danceId))
            {
                if (danceId > 4 || danceId < 0)
                {
                    session.SendWhisper("O id vai de 1 ao 4!");
                    return;
                }
                thisUser.DanceId = danceId;
                session.GetHabbo().CurrentRoom.SendMessage(new DanceComposer(thisUser, danceId));
                return;
            }
            session.SendWhisper("Insira um valor válido.");
        }
    }
}