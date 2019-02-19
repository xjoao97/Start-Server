/*#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class roomDeclineOffer : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "";


        public string Description => "Recusar a oferta de quarto atual.";

        public void Execute(GameClient Session, string[] Params)
        {
            var CurrentRoom = Session.GetHabbo().CurrentRoom;
            var RoomOwner = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (!RoomOwner.RoomOfferPending) return;
            if (!RoomOwner.GetClient().GetHabbo().CurrentRoom.RoomData.RoomForSale) return;
            if (RoomOwner.GetClient().GetHabbo().CurrentRoom.RoomData.OwnerId != RoomOwner.GetClient().GetHabbo().Id)
                return;
            var OfferingUser = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(RoomOwner.RoomOfferUser);
//            OfferingUser.GetClient().SendWhisper("Este usuário recusou sua oferta.");
            RoomOwner.RoomOfferPending = false;
            RoomOwner.RoomOfferUser = 0;
            RoomOwner.RoomOffer = "";
        }
    }
}*/