using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync
{
    public class JiraFields
    {
        public Project project { get; set; }
        public IssueType issuetype { get; set; }
        public String summary;
        public String description;
    }
    public class JiraField
    {
    }

    public class Project: JiraField
    {
        public string key { get; set; }
    }
    public class IssueType : JiraField
    {
        public string name { get; set; }
    }
}
