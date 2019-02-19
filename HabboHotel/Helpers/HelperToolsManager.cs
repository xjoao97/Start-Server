/*#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Helpers
{
    public class HelperToolsManager
    {
        public List<HabboHelper> Helpers;
        public List<HelperCase> Cases;
        public int ANSWER_CALL_WAIT_TIME = 120;

        public int GuideCount => Helpers.Count(c => c.IsGuide);

        public int HelperCount => Helpers.Count(c => c.IsHelper);

        public int GuardianCount => Helpers.Count(c => c.IsGuardian);


        public void LoadList()
        {
            Helpers = new List<HabboHelper>();
            Cases = new List<HelperCase>();
        }

        public void Disconnect(GameClient Session)
        {
           // var client = Helpers.FirstOrDefault(c => c.Session == Session);

            var element = GetElement(Session);
            if (element == null)
                return;
            if (element is HabboHelper)
            {
                var h = (HabboHelper) element;
                RemoveHelper(h);

                h.Case?.End(0);

                h.InvinteCase?.OnDecline(h);
            }
            else if (element is HelperCase)
            {
                var c = (HelperCase) element;
                RemoveCall(c);
                c.Helper?.End(0);
            }
        }

        public HabboHelper AddHelper(GameClient Session, bool IsHelper, bool IsGard, bool IsGuide)
        {
            var h = GetHelper(Session);
            if (h != null)
                return h;
            h = new HabboHelper(Session, IsGuide, IsHelper, IsGard);
            Helpers.Add(h);

            return h;
        }

        public HelperCase AddCall(GameClient Session, string message, int category)
        {
            var c = GetCall(Session);
            if (c != null)
                return c;
            var hcase = new HelperCase(Session, message, category);
            Cases.Add(hcase);
            return hcase;
        }

        public HabboHelper GetHelper(GameClient Session) => Helpers.FirstOrDefault(c => c.Session == Session);


        public void RemoveHelper(HabboHelper client) => Helpers.Remove(client);

        public void RemoveCall(HelperCase Call) => Cases.Remove(Call);

        public void RemoveCall(GameClient client)
        {
            var call = GetCall(client);
            if (call != null)
                RemoveCall(call);
        }


        public void RemoveHelper(GameClient Session)
        {
            var h = GetHelper(Session);
            if (h != null)
                RemoveHelper(h);
        }

        public HelperCase GetCall(GameClient Session) => Cases.FirstOrDefault(c => c.Session == Session);

        public void InvinteHelpCall(HabboHelper Helper, HelperCase hcase)
        {
            Helper.InvinteCase = hcase;
            Helper.Session.SendMessage(new CallForHelperWindowComposer(true, hcase));
            hcase.Helper = Helper;
        }

        public IHelperElement GetElement(GameClient Session)
            => Cases.Union<IHelperElement>(Helpers).FirstOrDefault(c => c.Session == Session);


        public List<HabboHelper> GetAvaliableHelpers() => Helpers.Where(c => !c.Busy).ToList();

        public List<HabboHelper> GetHelpersToCase(HelperCase Case) => GetAvaliableHelpers()
            .Where(c => Case.DeclinedHelpers.All(d => d != c))
            .Where(
                c =>
                    (Case.Session != c.Session) &&
                    ((c.IsGuide && (Case.Type == HelpCaseType.MEET_HOTEL)) ||
                     (c.IsHelper && (Case.Type == HelpCaseType.INSTRUCTION))))
            .ToList();
    }
}*/

