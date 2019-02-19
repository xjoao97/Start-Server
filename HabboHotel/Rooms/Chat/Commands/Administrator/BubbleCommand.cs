#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms.Chat.Styles;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class BubbleCommand : IChatCommand
    {
        public string PermissionRequired => "command_bubble";

        public string Parameters => "%id%";

        public string Description => "Use um estilo de chat customizado.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            if (Params.Length == 1)
            {
                session.SendWhisper("Opa, você se esqueceu do id.");
                return;
            }

            var bubble = 0;
            if (!int.TryParse(Params[1], out bubble))
            {
                session.SendWhisper("Por favor, use um número válido.");
                return;
            }

            ChatStyle style = null;
            if (!OblivionServer.GetGame().GetChatManager().GetChatStyles().TryGetStyle(bubble, out style) ||
                style.RequiredRight.Length > 0 && !session.GetHabbo().GetPermissions().HasRight(style.RequiredRight))
            {
                session.SendWhisper("Seu rank não permite que você use este estilo de chat.");
                return;
            }

            user.LastBubble = bubble;
            session.GetHabbo().CustomBubbleId = bubble;
            session.SendWhisper("Estilo de chat definido: " + bubble);
        }
    }
}