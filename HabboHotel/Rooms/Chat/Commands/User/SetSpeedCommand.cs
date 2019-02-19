#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class SetSpeedCommand : IChatCommand
    {
        public string PermissionRequired => "command_setspeed";

        public string Parameters => "%value%";

        public string Description => "Defina a velocidade dos rollers no quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (!room.CheckRights(session, true))
                return;

            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira uma velocidade.");
                return;
            }

            double speed;
            if (double.TryParse(Params[1], out speed))
                session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
            else
                session.SendWhisper("Quantidade inválida, insira um valor válido.");
        }
    }
}