#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms.Games.Teams;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class EnableCommand : IChatCommand
    {
        public string PermissionRequired => "command_enable";

        public string Parameters => "%EffectId%";

        public string Description => "Te dá habilidade de usar um efeito!";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Escolha um ID de efeito!");
                return;
            }

            var thisUser =
                session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);

            if (thisUser == null)
                return;


            if (thisUser.RidingHorse)
            {
                session.SendWhisper("Você não pode usar efeitos enquanto monta em um cavalo!");
                return;
            }
            if (thisUser.Team != TEAM.NONE || thisUser.isLying)
                return;

            int effectId;
            if (!int.TryParse(Params[1], out effectId))
                return;

            if ((effectId == 102 || effectId == 187) && !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper("Apenas staffs podem usar estes efeitos.");
                return;
            }

            if (effectId == 178 && !session.GetHabbo().GetPermissions().HasRight("gold_vip") &&
                !session.GetHabbo().GetPermissions().HasRight("events_staff"))
            {
                session.SendWhisper("Opa, apenas usuários VIP podem utilizar este efeito.");
                return;
            }

            session.GetHabbo().Effects().ApplyEffect(effectId);
        }
    }
}