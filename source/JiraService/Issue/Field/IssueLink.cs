using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraService.Issue.Field
{
    public class Type
    {
        public string id { get; set; }
        public string name { get; set; }
        public string inward { get; set; }
        public string outward { get; set; }
    }

    public class Status
    {
        public string iconUrl { get; set; }
        public string name { get; set; }
    }

    public class Fields
    {
        public Status status { get; set; }
    }

    public class Issuelink
    {
        public string id { get; set; }
        public Type type { get; set; }
        public Issue outwardIssue { get; set; }
        public Issue inwardIssue { get; set; }
    }
}
