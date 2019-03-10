using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Rooms.Camera;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.Utilities;

namespace Oblivion.Communication.Packets.Incoming.Rooms.Camera
{
    public class PurchasePhotoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom ||
                Session.GetHabbo().Credits < OblivionServer.GetGame().GetCameraManager().PurchaseCoinsPrice ||
                Session.GetHabbo().Duckets < OblivionServer.GetGame().GetCameraManager().PurchaseDucketsPrice)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User?.LastPhotoPreview == null)
                return;

            var preview = User.LastPhotoPreview;

            if (OblivionServer.GetGame().GetCameraManager().PurchaseCoinsPrice > 0)
            {
                Session.GetHabbo().Credits -= OblivionServer.GetGame().GetCameraManager().PurchaseCoinsPrice;
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
            }

            if (OblivionServer.GetGame().GetCameraManager().PurchaseDucketsPrice > 0)
            {
                Session.GetHabbo().Duckets -= OblivionServer.GetGame().GetCameraManager().PurchaseDucketsPrice;
                Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets,
                    Session.GetHabbo().Duckets));
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `camera_photos` SET `file_state` = 'purchased' WHERE `id` = '" + preview.Id +
                                  "' LIMIT 1");
            }

            var photoPoster =
                ItemFactory.CreateSingleItemNullable(OblivionServer.GetGame().GetCameraManager().PhotoPoster,
                    Session.GetHabbo(),
                    "{\"w\":\"" +
                    StringCharFilter.EscapeJSONString(
                        OblivionServer.GetGame()
                            .GetCameraManager()
                            .GetPath(CameraPhotoType.PURCHASED, preview.Id, preview.CreatorId)) + "\", \"n\":\"" +
                    StringCharFilter.EscapeJSONString(Session.GetHabbo().Username) + "\", \"s\":\"" +
                    Session.GetHabbo().Id + "\", \"u\":\"" + preview.Id + "\", \"t\":\"" + preview.CreatedAt + "\"}",
                    "");

            if (photoPoster != null)
            {
                Session.GetHabbo().GetInventoryComponent().TryAddItem(photoPoster);

                Session.SendMessage(new FurniListAddComposer(photoPoster));
                Session.SendMessage(new FurniListUpdateComposer());
            }

            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_CameraPhotoCount", 1);

            Session.SendMessage(new CameraPhotoPurchaseOkComposer());
        }
    }
}