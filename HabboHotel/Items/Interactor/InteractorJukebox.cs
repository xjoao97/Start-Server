#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Items.Interactor
{
    internal class InteractorJukebox : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.GetRoom().GetTraxManager().ClearPlayList();
            Item.ExtraData = "0";
            Item.UpdateState();
        }

        public void OnWiredTrigger(Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            var room = Item.GetRoom();
            var insideJukebox = Request == 0 || Request == 1;
            if (insideJukebox)
                room.GetTraxManager().TriggerPlaylistState();
        }
    }
}