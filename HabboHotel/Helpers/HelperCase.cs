/*#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Helpers
{
    public class HelperCase : IHelperElement
    {
        public List<HabboHelper> DeclinedHelpers;
        private readonly int Expire;
        public HabboHelper Helper;
        public string Message;
        public HelpCaseType Type;

        public HelperCase(GameClient client, string msg, int category)
        {
            Session = client;
            Message = msg;
            Type = (HelpCaseType) category;
            Expire = (int) OblivionServer.GetUnixTimestamp() + OblivionServer.GetGame().GetHelperManager().ANSWER_CALL_WAIT_TIME;
            DeclinedHelpers = new List<HabboHelper>();
        }

        public int ReamingToExpire => Expire - (int) OblivionServer.GetUnixTimestamp();
        public GameClient Session { get; set; }


        public IHelperElement OtherElement => Helper;

        public void End(int ErrorCode = 1) => Session.SendMessage(new EndHelperSessionComposer(ErrorCode));

        public void Close()
        {
            OblivionServer.GetGame().GetHelperManager().RemoveCall(this);
            Session.SendMessage(new CloseHelperSessionComposer());
        }

        public void OnDecline(HabboHelper Helper)
        {
            DeclinedHelpers.Add(Helper);

            var newhelper = OblivionServer.GetGame().GetHelperManager().GetHelpersToCase(this).FirstOrDefault();
            if (newhelper != null)
                OblivionServer.GetGame().GetHelperManager().InvinteHelpCall(newhelper, this);
            else
            {
                Session.SendMessage(new CallForHelperErrorComposer(1));
                OblivionServer.GetGame().GetHelperManager().RemoveCall(this);
            }
        }
    }

    public enum HelpCaseType
    {
        MEET_HOTEL = 0,
        INSTRUCTION = 1,
        EMERGENCY = 2
    }
}*/ /**/

