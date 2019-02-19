using Oblivion.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class LogMeInCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_logmein"; }
        }
        public string Parameters
        {
            get { return "%username% %password%"; }
        }
        public string Description
        {
            get { return "Logue-se como STAFF."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Algo está faltando!");
                return;
            }

            if (Session.GetHabbo().isLoggedIn == true)
            {
                Session.SendWhisper("Você já está logado.");
                return;
            }

            if (Params[1] != Session.GetHabbo().Username)
            {
                Session.SendWhisper("Só pode iniciar sessão na sua conta.");
                return;
            }

            if (Session.GetHabbo().Username == Params[1])
            {
                string passw = Params[2];
                string password;

                using (IQueryAdapter dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `password` FROM stafflogin WHERE `user_id` = " + Session.GetHabbo().Id + " LIMIT 1");
                    dbClient.AddParameter("password", passw);
                    password = dbClient.getString();
                }

                if (password == Params[2])
                {
                    Session.GetHabbo().isLoggedIn = true;
                    Session.SendWhisper("Bem-vindo " + Params[1] + ", agora você está logado como staff!");
                }
                else if (password != Params[2])
                {
                    Session.SendWhisper("Senha incorreta.");
                }
            }
        }
    }
}
