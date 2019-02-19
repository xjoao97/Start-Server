#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class Mp5Command : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "";

        public string Description => "Use sua Mp5.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var roomUserByHabbo1 = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUserByHabbo1 == null)
                return;

            room.SendMessage(new ChatComposer(roomUserByHabbo1.VirtualId, "*Puxa a mp5*", 0, 0));
            roomUserByHabbo1.ApplyEffect(541);
        }
    }
}