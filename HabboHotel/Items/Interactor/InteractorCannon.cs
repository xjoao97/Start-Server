#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Interactor
{
    internal class InteractorCannon : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session?.GetHabbo() == null || Item == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var Actor = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (Actor == null)
                return;

            if (Item.ExtraData == "1")
                return;

            if (Gamemap.TileDistance(Actor.X, Actor.Y, Item.GetX, Item.GetY) > 2)
                return;

            Item.ExtraData = "1";
            Item.UpdateState(false, true);

            Item.RequestUpdate(2, true);
        }

        public void OnWiredTrigger(Item Item)
        {
            if (Item == null)
                return;

            if (Item.ExtraData == "1")
                return;

            Item.ExtraData = "1";
            Item.UpdateState(false, true);

            Item.RequestUpdate(2, true);
        }
    }
}