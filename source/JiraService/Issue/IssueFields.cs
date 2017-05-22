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
    }
}
