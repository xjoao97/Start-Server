#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.Wired;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Wired;

#endregion

namespace Oblivion.HabboHotel.Items.Interactor
{
    public class InteractorWired : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            //Room Room = Item.GetRoom();
            //Room.GetWiredHandler().RemoveWired(Item);
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null || Item == null)
                return;

            if (!HasRights)
                return;

            IWiredItem Box = null;
            if (!Item.GetRoom().GetWired().TryGet(Item.Id, out Box))
                return;


            Item.ExtraData = "1";
            Item.UpdateState(false, true);
            Item.RequestUpdate(2, true);

            if (Item.GetBaseItem().WiredType == WiredBoxType.AddonRandomEffect ||
                Item.GetBaseItem().WiredType == WiredBoxType.EffectFixRoom ||
                Item.GetBaseItem().WiredType == WiredBoxType.EffectGiveUserFreeze ||
                Item.GetBaseItem().WiredType == WiredBoxType.EffectUserFastWalk)
                return;
            switch (Item.GetBaseItem().InteractionType)
            {
                case InteractionType.WiredCustom:
                case InteractionType.WiredTrigger:
                    var BlockedItems = WiredBoxTypeUtility.ContainsBlockedEffect(Box,
                        Item.GetRoom().GetWired().GetEffects(Box));
                    Session.SendMessage(new WiredTriggerConfigComposer(Box, BlockedItems));
                    break;
                case InteractionType.WiredEffect:
                    var Items = WiredBoxTypeUtility.ContainsBlockedTrigger(Box,
                        Item.GetRoom().GetWired().GetTriggers(Box));
                    Session.SendMessage(new WiredEffectConfigComposer(Box, Items));
                    break;
                case InteractionType.WiredCondition:
                    Session.SendMessage(new WiredConditionConfigComposer(Box));
                    break;
            }
        }


        public void OnWiredTrigger(Item Item)
        {
        }
    }
}