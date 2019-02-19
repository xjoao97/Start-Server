#region

using log4net;
using Oblivion.HabboHotel.Rooms.Chat.Commands;
using Oblivion.HabboHotel.Rooms.Chat.Emotions;
using Oblivion.HabboHotel.Rooms.Chat.Filter;
using Oblivion.HabboHotel.Rooms.Chat.Logs;
using Oblivion.HabboHotel.Rooms.Chat.Pets.Commands;
using Oblivion.HabboHotel.Rooms.Chat.Pets.Locale;
using Oblivion.HabboHotel.Rooms.Chat.Styles;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat
{
    public sealed class ChatManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Rooms.Chat.ChatManager");

        /// <summary>
        ///     Chat styles.
        /// </summary>
        private readonly ChatStyleManager _chatStyles;

        /// <summary>
        ///     Commands.
        /// </summary>
        private readonly CommandManager _commands;

        /// <summary>
        ///     Chat Emoticons.
        /// </summary>
        private readonly ChatEmotionsManager _emotions;

        /// <summary>
        ///     Filter Manager.
        /// </summary>
        private readonly WordFilterManager _filter;

        /// <summary>
        ///     Chatlog Manager
        /// </summary>
        private readonly ChatlogManager _logs;

        /// <summary>
        ///     Pet Commands.
        /// </summary>
        private readonly PetCommandManager _petCommands;

        /// <summary>
        ///     Pet Locale.
        /// </summary>
        private readonly PetLocale _petLocale;

        /// <summary>
        ///     Initializes a new instance of the ChatManager class.
        /// </summary>
        public ChatManager()
        {
            _emotions = new ChatEmotionsManager();
            _logs = new ChatlogManager();

            _filter = new WordFilterManager();
            _filter.Init();

            _commands = new CommandManager(":");
            _petCommands = new PetCommandManager();
            _petLocale = new PetLocale();

            _chatStyles = new ChatStyleManager();
            _chatStyles.Init();

            log.Info("Chat Manager -> LOADED");
        }

        public ChatEmotionsManager GetEmotions() => _emotions;

        public ChatlogManager GetLogs() => _logs;

        public WordFilterManager GetFilter() => _filter;

        public CommandManager GetCommands() => _commands;

        public PetCommandManager GetPetCommands() => _petCommands;

        public PetLocale GetPetLocale() => _petLocale;

        public ChatStyleManager GetChatStyles() => _chatStyles;
    }
}