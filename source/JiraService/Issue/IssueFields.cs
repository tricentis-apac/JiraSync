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
        public ProjectField Project { get; set; }
        public IssueTypeField IssueType { get; set; }
    }
}
