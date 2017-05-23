using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync
{
    public static class Global
    {
        public const String JiraTicketAttributeName = "JiraTicket";
        public const String JiraLastSyncedAttributeName = "JiraLastSync";
        public const String JiraSyncStateAttributeName = "JiraSyncState";

        public const String JiraBaseURL = "JiraBaseURL";
        public const String JiraBaseJQL = "JiraJQL";

        public const String JiraDefectID = "JiraDefectID";
        public const String JiraDefectKey = "JiraDefectKey";
        public const String JiraDefectURL = "JiraDefectURL";
        public const String JiraDefectProject = "JiraDefectProjectKey";

        public enum JiraSyncStates
        {
            Updated,
            NotUpdated
        }
    }
}
