#region

using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class RegenMaps : IChatCommand
    {
        public string PermissionRequired => "command_regen_maps";

        public string Parameters => "";

        public string Description => Language.GetValue("fixroom.desc");

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Hey, apenas o dono do quarto pode utilizar esse comando.");
                return;
            }
            
            room.GetGameMap().GenerateMaps();
            session.SendWhisper("Feito!");
        }
    }
}