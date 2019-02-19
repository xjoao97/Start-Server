#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Items.Interactor
{
    public class InteractorTeleport : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.AllowOverride = false;
                    User.CanWalk = true;
                }

                Item.InteractingUser = 0;
            }

            if (Item.InteractingUser2 != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser2);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.AllowOverride = false;
                    User.CanWalk = true;
                }

                Item.InteractingUser2 = 0;
            }
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                User?.UnlockWalking();

                Item.InteractingUser = 0;
            }

            if (Item.InteractingUser2 != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser2);

                User?.UnlockWalking();

                Item.InteractingUser2 = 0;
            }
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Item?.GetRoom() == null || Session?.GetHabbo() == null)
                return;

            var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            User.LastInteraction = OblivionServer.GetUnixTimestamp();

            // Alright. But is this user in the right position?
            if (User.Coordinate == Item.Coordinate || User.Coordinate == Item.SquareInFront)
            {
                // Fine. But is this tele even free?
                if (Item.InteractingUser != 0)
                    return;

                if (!User.CanWalk || Session.GetHabbo().IsTeleporting || Session.GetHabbo().TeleporterId != 0 ||
                    User.LastInteraction + 2 - OblivionServer.GetUnixTimestamp() < 0)
                    return;

                User.TeleDelay = 2;

                Item.InteractingUser = User.GetClient().GetHabbo().Id;
            }
            else if (User.CanWalk)
            {
                User.MoveTo(Item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}