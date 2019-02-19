#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator;
using Oblivion.HabboHotel.Rooms.Chat.Commands.Events;
using Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator;
using Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;
using Oblivion.HabboHotel.Rooms.Chat.Commands.User;
using Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands
{
    public class CommandManager
    {
        /// <summary>
        ///     Commands registered for use.
        /// </summary>
        internal static Dictionary<string, IChatCommand> Commands;

        /// <summary>
        ///     Command Prefix only applies to custom commands.
        /// </summary>
        private readonly string _prefix;

        /// <summary>
        ///     The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string Prefix)
        {
            _prefix = Prefix;
            Commands = new Dictionary<string, IChatCommand>();

            RegisterVIP();
            RegisterUser();
            RegisterEvents();
            RegisterModerator();
            RegisterAdministrator();
        }

        /// <summary>
        ///     Request the text to parse and check for commands that need to be executed.
        /// </summary>
        /// <param name="Session">Session calling this method.</param>
        /// <param name="Message">The message to parse.</param>
        /// <returns>True if parsed or false if not.</returns>
        public bool Parse(GameClient Session, string Message)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return false;

            if (!Message.StartsWith(_prefix))
                return false;

            if (Message == _prefix + Language.GetValue("command.list"))
            {
                var List = new StringBuilder();
                List.Append(Language.GetValue("command.list.desc") + "\n");
                foreach (var CmdList in Commands.ToList())
                {
                    if (!string.IsNullOrEmpty(CmdList.Value.PermissionRequired))
                        if (!Session.GetHabbo().GetPermissions().HasCommand(CmdList.Value.PermissionRequired))
                            continue;

                    List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n········································································\n");

                }
                Session.SendMessage(new MotdNotificationComposer(List.ToString()));
                return true;
            }

            Message = Message.Substring(1);
            var Split = Message.Split(' ');

            if (Split.Length == 0)
                return false;

            var blockedcommands = Session.GetHabbo().CurrentRoom.RoomData.BlockedCommands;

            if (blockedcommands.Contains(Split[0].ToLower()) && Session.GetHabbo().Rank < 5)
            {
                Session.SendWhisper(Language.GetValue("command.blocked"));
                return false;
            }

            if (Session.GetHabbo().BlockedCommands.Contains(Split[0].ToLower()) && Session.GetHabbo().Rank < 7)
            {
                Session.SendWhisper(Language.GetValue("command.blocked"));
                return false;
            }

            IChatCommand Cmd;
            if (Commands.TryGetValue(Split[0].ToLower(), out Cmd))
            {
                if (Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId);

                if (!string.IsNullOrEmpty(Cmd.PermissionRequired))
                    if (!Session.GetHabbo().GetPermissions().HasCommand(Cmd.PermissionRequired))
                        return false;


                Session.GetHabbo().IChatCommand = Cmd;
                Session.GetHabbo()
                    .CurrentRoom.GetWired()
                    .TriggerEvent(WiredBoxType.TriggerUserSaysCommand, Session.GetHabbo(), this);

                Cmd.Execute(Session, Split);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Registers the VIP set of commands.
        /// </summary>
        private void RegisterVIP()
        {
            Register(Language.GetValue("command.spush"), new SuperPushCommand());
            Register(Language.GetValue("command.spull"), new SuperPullCommand());
        }

        /// <summary>
        ///     Registers the Events set of commands.
        /// </summary>
        private void RegisterEvents()
        {
            Register("event", new EventAlertCommand());
            Register("eventlist", new EventListCommand());
            Register("addevent", new AddEventCommand());
            Register("eventra", new EventRadio());
            Register("epoints", new GiveEventPoints());
            Register("newrares", new CatalogUpdateAlert());
            Register("locutoron", new DjOnline());
            Register("rpoints", new GiveRadioPoints());
            Register("ppoints", new GivePromoPoints());
        }

        /// <summary>
        ///     Registers the default set of commands.
        /// </summary>
        private void RegisterUser()
        {
            Register("handitem", new CarryCommand());
            Register("oblivion", new OblivionCommand());
            Register(Language.GetValue("command.about"), new InfoCommand());
            Register("pickall", new PickAllCommand());
            Register(Language.GetValue("command.eventsoff"), new DisableEvent());
            Register("eventsoff", new DisableEvent());
            Register("disablecmd", new DisableCommands());
            Register("sit", new SitCommand());
            Register("lay", new LayCommand());
            Register("color", new ColourCommand());
            Register("stand", new StandCommand());
            Register("mutepets", new MutePetsCommand());
            Register("mutebots", new MuteBotsCommand());
            Register("mimic", new MimicCommand());
            Register("dance", new DanceCommand());
            Register("push", new PushCommand());
            Register("pull", new PullCommand());
            Register("enable", new EnableCommand());
            Register("follow", new FollowCommand());
            Register("faceless", new FacelessCommand());
            Register("moonwalk", new MoonwalkCommand());
            Register(Language.GetValue("command.cut"), new CutCommand());
            Register("facepalm", new FacepalmCommand());
            Register(Language.GetValue("command.sex"), new SexCommand());
            Register(Language.GetValue("command.hug"), new HugCommand());
            Register(Language.GetValue("command.jerk"), new JerkOffCommand());
            Register("mp5", new Mp5Command());
            Register(Language.GetValue("command.smoke"), new WeedCommand());
            Register("buyroom", new roomBuyCommand());
            Register("startquestion", new StartQuickPollCommand()); 
            Register("stopquestion", new StopQuickPollCommand());
            Register("sellroom", new RoomSellCommand());
            Register("welcome", new WelcomeCommand());
            Register("onlines", new OnlineCommand());

            Register("unload", new UnloadCommand());
            Register(Language.GetValue("command.fixroom"), new RegenMaps());
            Register("empty", new EmptyItems());
            Register("setmax", new SetMaxCommand());
            Register("setspeed", new SetSpeedCommand());
            Register("disablefriends", new DisableFriendsCommand());
            Register("enablefriends", new EnableFriendsCommand());
            Register("disablediagonal", new DisableDiagonalCommand());
            Register("flagme", new FlagMeCommand());

            Register("stats", new StatsCommand());
            Register("kickpets", new KickPetsCommand());
            Register("kickbots", new KickBotsCommand());

            Register("dnd", new DndCommand());
            Register("kill", new KillCommand());
            Register("disco", new DiscoCommand());
            Register("disablegifts", new DisableGiftsCommand());
            Register("convertcredits", new ConvertCreditsCommand());
            Register("disablewhispers", new DisableWhispersCommand());
            Register("disablemimic", new DisableMimicCommand());
            Register("group", new GroupChatCommand());
            Register("groupnotif", new GroupAlertCommand());
            Register("pet", new PetCommand());
        }

        /// <summary>
        ///     Registers the moderator set of commands.
        /// </summary>
        private void RegisterModerator()
        {
            Register("ban", new BanCommand());
            Register("mip", new MipCommand());
            Register("ipban", new IpBanCommand());
            Register("disableusercmd", new DisableUserCommand());
            Register("tile", new FloorTileCommand());

            Register("ui", new UserInfoCommand());
            Register("userinfo", new UserInfoCommand());
            Register("roomcredits", new GiveRoom());
            Register("sa", new StaffAlertCommand());
            Register("roomunmute", new RoomUnmuteCommand());
            Register("roommute", new RoomMuteCommand());
            Register("roombadge", new RoomBadgeCommand());
            Register("roomalert", new RoomAlertCommand());
            Register("roomkick", new RoomKickCommand());
            Register("mute", new MuteCommand());
            Register("smute", new MuteCommand());
            Register("unmute", new UnmuteCommand());
            Register("massbadge", new MassBadgeCommand());
            Register("massgive", new MassGiveCommand());
            Register("globalgive", new GlobalGiveCommand());
            Register("removebadge", new RemoveBadgeCommand());
            Register("kick", new KickCommand());
            Register("skick", new KickCommand());
            Register("ha", new HotelAlertCommand());
            Register("notification", new NotificationAlert());
            Register("hal", new HalCommand());
            Register("enviar", new GiveCommand());
            Register("givebadge", new GiveBadgeCommand());
            Register("dc", new DisconnectCommand());
            Register("alert", new AlertCommand());
            Register("tradeban", new TradeBanCommand());
            Register("teleport", new TeleportCommand());
            Register("summon", new SummonCommand());
            Register("override", new OverrideCommand());
            Register("massenable", new MassEnableCommand());
            Register("massdance", new MassDanceCommand());
            Register("freeze", new FreezeCommand());
            Register("unfreeze", new UnFreezeCommand());
            Register("fastwalk", new FastwalkCommand());
            Register("superfastwalk", new SuperFastwalkCommand());
            Register("coords", new CoordsCommand());
            Register("alleyesonme", new AllEyesOnMeCommand());
            Register("allaroundme", new AllAroundMeCommand());
            Register("forcesit", new ForceSitCommand());
            Register("ignorewhispers", new IgnoreWhispersCommand());
            Register("forced_effects", new DisableForcedFxCommand());
            Register("makesay", new MakeSayCommand());
            Register("flaguser", new FlagUserCommand());
            Register("verclones", new VerClonesCommand());
        }

        /// <summary>
        ///     Registers the administrator set of commands.
        /// </summary>
        private void RegisterAdministrator()
        {
            Register("bubble", new BubbleCommand());
            Register("update", new UpdateCommand());
            Register("emptyuser", new EmptyUser());
            Register("deletegroup", new DeleteGroupCommand());
            Register("goto", new GotoCommand());
            Register("enquete", new EnqueteCommand());
        }

        /// <summary>
        ///     Registers a Chat Command.
        /// </summary>
        /// <param name="CommandText">Text to type for this command.</param>
        /// <param name="Command">The command to execute.</param>
        public void Register(string CommandText, IChatCommand Command) => Commands.Add(CommandText, Command);

        public static string MergeParams(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (var i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                    Merged.Append(" ");
                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }

        public void LogCommand(int UserId, string Data, string MachineId)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `logs_client_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", UserId);
                dbClient.AddParameter("Data", Data);
                dbClient.AddParameter("MachineId", MachineId);
                dbClient.AddParameter("Timestamp", OblivionServer.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public bool TryGetCommand(string Command, out IChatCommand IChatCommand)
            => Commands.TryGetValue(Command, out IChatCommand);
    }
}