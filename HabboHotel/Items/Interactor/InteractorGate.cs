#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Wired;

#endregion

namespace Oblivion.HabboHotel.Items.Interactor
{
    internal class InteractorGate : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (!hasRights)
                return;
            if (item?.GetBaseItem() == null || item.GetBaseItem().InteractionType != InteractionType.Gate)
                return;
            var modes = item.GetBaseItem().Modes - 1;
            if (modes <= 0)
                item.UpdateState(false, true);

            if (item.GetRoom() == null || item.GetRoom().GetGameMap() == null ||
                item.GetRoom().GetGameMap().SquareHasUsers(item.GetX, item.GetY))
                return;

            int currentMode;
            int.TryParse(item.ExtraData, out currentMode);

            int newMode;
            if (currentMode <= 0)
                newMode = 1;
            else if (currentMode >= modes)
                newMode = 0;
            else
                newMode = currentMode + 1;
            //  ConsoleWriter.Writer.WriteLine(newMode.ToString());

            if (newMode == 0 && !item.GetRoom().GetGameMap().itemCanBePlacedHere(item.GetX, item.GetY))
                return;

            if (
                item.GetRoom()
                    .GetGameMap()
                    .Walkingtofurni(
                        item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username), item) &&
                newMode == 0)
                item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username).ClearMovement(true);

            item.ExtraData = newMode.ToString();
            item.UpdateState();
            item.GetRoom().GetGameMap().updateMapForItem(item);
            item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, session.GetHabbo(), item);
            // ConsoleWriter.Writer.WriteLine("Closed");
            // 
        }

        public void OnWiredTrigger(Item item)
        {
//todo: fix this like ^

            var num = item.GetBaseItem().Modes - 1;
            if (num <= 0)
                item.UpdateState(false, true);
            if (item.GetRoom() == null || item.GetRoom().GetGameMap() == null ||
                item.GetRoom().GetGameMap().SquareHasUsers(item.GetX, item.GetY))
                return;
            var num2 = 0;
            int.TryParse(item.ExtraData, out num2);
            int num3;
            if (num2 <= 0)
            {
                num3 = 1;
            }
            else
            {
                if (num2 >= num)
                    num3 = 0;
                else
                    num3 = num2 + 1;
            }
            if (num3 == 0 && !item.GetRoom().GetGameMap().itemCanBePlacedHere(item.GetX, item.GetY))
                return;

            item.ExtraData = num3.ToString();
            item.UpdateState();
            item.GetRoom().GetGameMap().updateMapForItem(item);
        }
    }
}