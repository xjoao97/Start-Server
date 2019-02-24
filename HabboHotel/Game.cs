#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.Communication.Packets;
using Oblivion.Core;
using Oblivion.HabboHotel.Achievements;
using Oblivion.HabboHotel.Bots;
using Oblivion.HabboHotel.Cache;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.Catalog.FurniMatic;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Games;
using Oblivion.HabboHotel.Global;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Groups.Forums;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Televisions;
using Oblivion.HabboHotel.LandingView;
using Oblivion.HabboHotel.Moderation;
using Oblivion.HabboHotel.Navigator;
using Oblivion.HabboHotel.Permissions;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rewards;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Chat;
using Oblivion.HabboHotel.Rooms.TraxMachine;

#endregion

namespace Oblivion.HabboHotel
{
    public class Game
    {
        internal bool ClientManagerCycleEnded, RoomManagerCycleEnded;
        private const int CycleSleepTime = 25;
        private static GroupForumManager _groupForumManager;
        //  private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Game");
        private readonly AchievementManager _achievementManager;
        private readonly AntiMutant _antiMutant;
        private readonly BotManager _botManager;
        private readonly CacheManager _cacheManager;
        //  private readonly HelperToolsManager _helperToolsManager;
        private readonly CameraPhotoManager _cameraManager;
        private readonly CatalogManager _catalogManager;
        private readonly ChatManager _chatManager;
        private readonly GameClientManager _clientManager;
        private readonly FurniMaticRewardsManager _furniMaticRewardsManager;
        private readonly GameDataManager _gameDataManager;
        private readonly ServerStatusUpdater _globalUpdater;
        private readonly GroupManager _groupManager;

        private readonly ItemDataManager _itemDataManager;
        private readonly LandingViewManager _landingViewManager; //TODO: Rename class
        private readonly LanguageLocale _languageLocale;
        private readonly ModerationManager _modManager;
        private readonly NavigatorManager _navigatorManager;

        private readonly PacketManager _packetManager;
        private readonly PermissionManager _permissionManager;
        private readonly QuestManager _questManager;
        private readonly RewardManager _rewardManager;
        private readonly RoomManager _roomManager;
//        private readonly SubscriptionManager _subscriptionManager;
//        private readonly TalentTrackManager _talentTrackManager;
        private readonly TelevisionManager _televisionManager; //TODO: Init from the item manager.
        private readonly TraxSoundManager _traxSoundManager;
        private bool _cycleActive;
        private bool _cycleEnded;
        private Task _gameCycle;
        public static int SessionUserRecord;
        public Game()
        {
            SessionUserRecord = 0;
            _packetManager = new PacketManager();
            _clientManager = new GameClientManager();
            _modManager = new ModerationManager();
            _furniMaticRewardsManager = new FurniMaticRewardsManager();
            _furniMaticRewardsManager.Init(OblivionServer.GetDatabaseManager().GetQueryReactor());

            _itemDataManager = new ItemDataManager();
            _itemDataManager.Init();

            _catalogManager = new CatalogManager();
            _catalogManager.Init(_itemDataManager);

            _televisionManager = new TelevisionManager();
            _cameraManager = new CameraPhotoManager();
            _cameraManager.Init(_itemDataManager);
            _navigatorManager = new NavigatorManager();
            _roomManager = new RoomManager();
            _chatManager = new ChatManager();
            _groupManager = new GroupManager();
            _groupForumManager = new GroupForumManager();
            _questManager = new QuestManager();
            _achievementManager = new AchievementManager();
//            _talentTrackManager = new TalentTrackManager();

            _landingViewManager = new LandingViewManager();
            _gameDataManager = new GameDataManager();

            _globalUpdater = new ServerStatusUpdater();
            _globalUpdater.Init();


            _languageLocale = new LanguageLocale();
            _antiMutant = new AntiMutant();
            _botManager = new BotManager();

            _cacheManager = new CacheManager();
            _rewardManager = new RewardManager();

            _permissionManager = new PermissionManager();
            _permissionManager.Init();

            _traxSoundManager = new TraxSoundManager();
            _traxSoundManager.Init();

            //            _subscriptionManager = new SubscriptionManager(); //todo: code hc?
            //            _subscriptionManager.LoadList();
            //  OblivionServer.GetGame().GetHelperManager().LoadList();
            //            _helperToolsManager = new HelperToolsManager();
            //            _helperToolsManager.LoadList();
        }

        public FurniMaticRewardsManager GetFurniMaticRewardsManager() => _furniMaticRewardsManager;

        public CameraPhotoManager GetCameraManager() => _cameraManager;

        public void StartGameLoop()
        {
            _gameCycle = new Task(GameCycle);
            _gameCycle.Start();

            _cycleActive = true;
        }

        private void GameCycle()
        {
            while (_cycleActive)
            {
                try
                {
                    _cycleEnded = false;

                    RoomManagerCycleEnded = false;
                    OblivionServer.GetGame().GetRoomManager().OnCycle();
                    OblivionServer.GetGame().GetClientManager().OnCycle();

                    _cycleEnded = true;
                }
                catch (Exception e)
                {
                    Logging.WriteLine(e.ToString());
                }
                Thread.Sleep(CycleSleepTime);
            }
        }

        public void StopGameLoop()
        {
            _cycleActive = false;

            while (!_cycleEnded || !RoomManagerCycleEnded) Thread.Sleep(25);
        }

        public PacketManager GetPacketManager() => _packetManager;

        public GameClientManager GetClientManager() => _clientManager;

        public CatalogManager GetCatalog() => _catalogManager;

        public NavigatorManager GetNavigator() => _navigatorManager;

        public ItemDataManager GetItemManager() => _itemDataManager;

        public RoomManager GetRoomManager() => _roomManager;

        public AchievementManager GetAchievementManager() => _achievementManager;
//        public TalentTrackManager GetTalentTrackManager() => _talentTrackManager;
//        public HelperToolsManager GetHelperManager() => _helperToolsManager;


        public ServerStatusUpdater GetServerStatusUpdater() => _globalUpdater;
        public TraxSoundManager GetSoundManager() => _traxSoundManager;


        public ModerationManager GetModerationManager() => _modManager;

        public PermissionManager GetPermissionManager() => _permissionManager;

        //        public SubscriptionManager GetSubscriptionManager() => _subscriptionManager;

        public QuestManager GetQuestManager() => _questManager;

        public GroupManager GetGroupManager() => _groupManager;
        public GroupForumManager GetGroupForumManager() => _groupForumManager;

        public LandingViewManager GetLandingManager() => _landingViewManager;

        public TelevisionManager GetTelevisionManager() => _televisionManager;

        public ChatManager GetChatManager() => _chatManager;

        public GameDataManager GetGameDataManager() => _gameDataManager;

        public LanguageLocale GetLanguageLocale() => _languageLocale;

        public AntiMutant GetAntiMutant() => _antiMutant;

        public BotManager GetBotManager() => _botManager;

        public CacheManager GetCacheManager() => _cacheManager;

        public RewardManager GetRewardManager() => _rewardManager;
    }
}