using System.Collections.Generic;
using System.Linq;
using log4net;
using Oblivion.HabboHotel.Items;

namespace Oblivion.HabboHotel.Camera
{
    public class CameraPhotoManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Camera.CameraPhotoManager");
        private int _maxPreviewCacheCount = 1000;

        private ItemData _photoPoster;

        private string _previewPath = "preview/{1}-{0}.png";

        private readonly Dictionary<int, CameraPhotoPreview> _previews;

        private string _purchasedPath = "purchased/{1}-{0}.png";

        public CameraPhotoManager()
        {
            _previews = new Dictionary<int, CameraPhotoPreview>();
        }

        public int PurchaseCoinsPrice { get; private set; } = 999;

        public int PurchaseDucketsPrice { get; private set; } = 999;

        public int PublishDucketsPrice { get; private set; } = 999;

        public ItemData PhotoPoster => _photoPoster;

        public void Init(ItemDataManager itemDataManager)
        {
            OblivionServer.GetConfig().data.TryGetValue("camera.path.preview", out _previewPath);
            OblivionServer.GetConfig().data.TryGetValue("camera.path.purchased", out _purchasedPath);

            if (OblivionServer.GetConfig().data.ContainsKey("camera.preview.maxcache"))
                _maxPreviewCacheCount = int.Parse(OblivionServer.GetConfig().data["camera.preview.maxcache"]);

            if (OblivionServer.GetDbConfig().DBData.ContainsKey("camera.photo.purchase.price.coins"))
                PurchaseCoinsPrice = int.Parse(OblivionServer.GetDbConfig().DBData["camera.photo.purchase.price.coins"]);

            if (OblivionServer.GetDbConfig().DBData.ContainsKey("camera.photo.purchase.price.duckets"))
                PurchaseDucketsPrice =
                    int.Parse(OblivionServer.GetDbConfig().DBData["camera.photo.purchase.price.duckets"]);

            if (OblivionServer.GetDbConfig().DBData.ContainsKey("camera.photo.publish.price.duckets"))
                PublishDucketsPrice =
                    int.Parse(OblivionServer.GetDbConfig().DBData["camera.photo.publish.price.duckets"]);

            var ItemId = int.Parse(OblivionServer.GetDbConfig().DBData["camera.photo.purchase.item_id"]);

            if (!itemDataManager.TryGetItem(ItemId, out _photoPoster))
                log.Error("Couldn't load photo poster item " + ItemId + ", no furniture record found.");

            log.Info("Camera Photo Manager -> LOADED");
        }

        public CameraPhotoPreview GetPreview(int PhotoId) => !_previews.ContainsKey(PhotoId) ? null : _previews[PhotoId];

        public void AddPreview(CameraPhotoPreview preview)
        {
            if (_previews.Count >= _maxPreviewCacheCount)
                _previews.Remove(_previews.Keys.First());

            _previews.Add(preview.Id, preview);
        }

        public string GetPath(CameraPhotoType type, int PhotoId, int CreatorId)
        {
            var path = "{1}-{0}.png";

            switch (type)
            {
                case CameraPhotoType.PREVIEW:
                    path = _previewPath;
                    break;
                case CameraPhotoType.PURCHASED:
                    path = _purchasedPath;
                    break;
            }

            return string.Format(path, PhotoId, CreatorId);
        }
    }
}