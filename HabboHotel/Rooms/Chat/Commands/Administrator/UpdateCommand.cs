#region

using System.Linq;
using System.Threading;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class UpdateCommand : IChatCommand
    {
        public string PermissionRequired => "command_update";

        public string Parameters => "%variable%";

        public string Description => "Recarrega uma partição específica do Hotel.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Você deve incluir a palavra chave na frente de :update, exemplo: update catalog ");
                return;
            }

            var updateVariable = Params[1];
            switch (updateVariable.ToLower())
            {
                case "cata":
                case "catalog":
                case "catalogue":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_catalog"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }
                    new Thread(() =>
                    {
                        OblivionServer.GetGame().GetCatalog().Init(OblivionServer.GetGame().GetItemManager());
                        OblivionServer.GetGame().GetClientManager().SendMessage(new CatalogUpdatedComposer());
                        session.SendWhisper("Catálogo atualizado com sucesso.");
                    }).Start();
                    break;
                }

                case "items":
                case "furni":
                case "furniture":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_furni"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetItemManager().Init();
                    session.SendWhisper("Itens atualizados com sucesso.");
                    break;
                }

                case "models":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_models"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetRoomManager().LoadModels();
                    session.SendWhisper("Modelos de quarto atualizados com sucesso.");
                    break;
                }

                case "promotions":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_promotions"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetLandingManager().LoadPromotions();
                    session.SendWhisper("Vista de quarto atualizada com sucesso.");
                    break;
                }

                case "youtube":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_youtube"))
                    {
                        session.SendWhisper("Sem permissão");
                        break;
                    }

                    OblivionServer.GetGame().GetTelevisionManager().Init();
                    session.SendWhisper("Playlist do Youtube atualizada com sucesso.");
                    break;
                }

                case "filter":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_filter"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetChatManager().GetFilter().Init();
                    session.SendWhisper("Definições de filtro atualizadas com sucesso.");
                    break;
                }

                case "navigator":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_navigator"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetNavigator().Init();
                    session.SendWhisper("Navegador atualizado com sucesso.");
                    break;
                }

                case "ranks":
                case "rights":
                case "permissions":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_rights"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetPermissionManager().Init();

                    foreach (
                        var client in
                        OblivionServer.GetGame()
                            .GetClientManager()
                            .GetClients.ToList()
                            .Where(client => client?.GetHabbo() != null && client.GetHabbo().GetPermissions() != null))
                        client.GetHabbo().GetPermissions().Init(client.GetHabbo());

                    session.SendWhisper("Definições de permissão atualizadas com sucesso.");
                    break;
                }

                case "config":
                case "settings":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_configuration"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.ConfigData = new ConfigData();
                    session.SendWhisper("Configurações do servidor atualizadas com sucesso.");
                    break;
                }

                case "bans":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_bans"))
                    {
                        session.SendWhisper("Sem permissão");
                        break;
                    }

                    OblivionServer.GetGame().GetModerationManager().ReCacheBans();
                    session.SendWhisper("Cachê de banimento limpo.");
                    break;
                }

                case "quests":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_quests"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetQuestManager().Init();
                    session.SendWhisper("Definições quest atualizadas com sucesso.");
                    break;
                }

                case "achievements":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_achievements"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetAchievementManager().LoadAchievements();
                    session.SendWhisper("Atualizado com sucessso.");
                    break;
                }

                case "moderation":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_moderation"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_moderation' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetModerationManager().Init();
                    OblivionServer.GetGame()
                        .GetClientManager()
                        .ModAlert(
                            "Moderation presets have been updated. Please reload the client to view the new presets.");

                    session.SendWhisper("Moderation configuration successfully updated.");
                    break;
                }

                case "tickets":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_tickets"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_tickets' permission.");
                        break;
                    }

                    if (OblivionServer.GetGame().GetModerationManager().GetModerationTool().Tickets.Count > 0)
                        OblivionServer.GetGame().GetModerationManager().GetModerationTool().Tickets.Clear();

                    OblivionServer.GetGame()
                        .GetClientManager()
                        .ModAlert("Tickets have been purged. Please reload the client.");
                    session.SendWhisper("Tickets successfully purged.");
                    break;
                }

                case "vouchers":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_vouchers"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_vouchers' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetCatalog().GetVoucherManager().Init();
                    session.SendWhisper("Catalogue vouche cache successfully updated.");
                    break;
                }

                case "gc":
                case "games":
                case "gamecenter":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_game_center"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_game_center' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetGameDataManager().Init();
                    session.SendWhisper("Game Center cache successfully updated.");
                    break;
                }

                case "pet_locale":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_pet_locale"))
                    {
                        session.SendWhisper("Sem permissão.");
                        break;
                    }

                    OblivionServer.GetGame().GetChatManager().GetPetLocale().Init();
                    session.SendWhisper("Definições pet atualizadas com sucesso.");
                    break;
                }

                case "locale":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_locale"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_locale' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetLanguageLocale().Init();
                    session.SendWhisper("Locale cache successfully updated.");
                    break;
                }

                case "mutant":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_anti_mutant"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_anti_mutant' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetAntiMutant().Init();
                    session.SendWhisper("Anti mutant successfully reloaded.");
                    break;
                }

                case "bots":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_bots"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_bots' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetBotManager().Init();
                    session.SendWhisper("Bot managaer successfully reloaded.");
                    break;
                }

                case "rewards":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_rewards"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_rewards' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetRewardManager().Reload();
                    session.SendWhisper("Rewards managaer successfully reloaded.");
                    break;
                }

                case "chat_styles":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_chat_styles"))
                    {
                        session.SendWhisper("Oops, you do not have the 'command_update_chat_styles' permission.");
                        break;
                    }

                    OblivionServer.GetGame().GetChatManager().GetChatStyles().Init();
                    session.SendWhisper("Chat Styles successfully reloaded.");
                    break;
                }
            }
        }
    }
}