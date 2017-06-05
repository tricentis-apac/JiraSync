using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync
{
    public static class Global
    {
        public const String JiraTicket = "JiraTicket";
        public const String JiraLastSyncedAttributeName = "JiraLastSync";
        public const String JiraSyncStateAttributeName = "JiraSyncState";

        public const String JiraURL = "JiraDefectURL";

        public const string JiraConfigAttachmentName = "JiraConfig.json";

        public enum JiraSyncStates
        {
            Updated,
            NotUpdated
        }
    }
}
