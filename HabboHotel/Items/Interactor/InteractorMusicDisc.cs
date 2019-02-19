#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Items.Interactor
{
    internal class InteractorMusicDisc : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            /*Item.GetRoom().GetTraxManager().ClearPlayList();
            Item.ExtraData = "0";
            Item.UpdateState();*/
        }

        public void OnRemove(GameClient Session, Item Item)
        {
//            var room = Item.GetRoom();
//            var cd = Item.GetRoom().GetTraxManager().GetDiscItem(Item.Id);
//            if (cd != null)
//            {
//                room.GetTraxManager().StopPlayList();
//                room.GetTraxManager().RemoveDisc(Item);
//            }
//            //else
//
//            var Items = room.GetTraxManager().GetAvaliableSongs();
//            Items.Remove(Item);
//            room.SendMessage(new LoadJukeboxUserMusicItemsComposer(Items));
        }

        public void OnWiredTrigger(Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
        }
    }
}