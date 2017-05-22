using JiraService.Issue.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraService.Issue
{
    public class IssueFields
    {
        public ProjectField project { get; set; }
        public IssueTypeField issuetype { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public Issuelink[] issuelinks { get; set; }
        public Issue[] subtasks { get; set; }
        public Issue parent { get; set; }
        public StatusField status { get; set; }
    }
}
