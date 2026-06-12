using System.Collections.Generic;
using System.Linq;

namespace _2.semEksamenProjekt
{
    public class Session
    {
        static Session instance;

        public static Session Instance
        {
            get
            {
                if (instance == null)
                    instance = new Session();
                return instance;
            }
        }

        public User CurrentUser;
        public List<Team> UserTeams = new List<Team>();

        // admin kan redigere events, flows og subflows
        public bool CanEditEvents => CurrentUser?.role == "Admin";

        // admin og underviser kan redigere flows og subflows
        public bool CanEditFlows => CurrentUser?.role == "Admin" || CurrentUser?.role == "Underviser";

        // underviser kan kun redigere subflows i flows de er tilknyttet
        // admin kan redigere alle
        public bool CanEditSubFlows(Flow flow)
        {
            if (CurrentUser?.role == "Admin") return true;

            if (CurrentUser?.role != "Underviser") return false;

            if (flow.teachers == null) return false;

            return flow.teachers.Any(t => t.username == CurrentUser.username);
        }

        // tjekker om et event er synligt for brugeren
        // studerende kan kun se events hvor deres hold er tilknyttet
        public bool CanSeeEvent(Event ev)
        {
            if (CurrentUser?.role != "Studerende") return true;

            if (ev.teams == null || ev.teams.Count == 0) return false;

            return ev.teams.Any(t => UserTeams.Any(ut => ut.teamName == t.teamName));
        }

        // tjekker om et flow er synligt for brugeren
        // studerende kan kun se flows der tilhører deres hold
        public bool CanSeeFlow(Flow flow)
        {
            if (CurrentUser?.role != "Studerende") return true;

            if (flow.team == null) return false;

            return UserTeams.Any(t => t.teamName == flow.team.teamName);
        }
    }
}
