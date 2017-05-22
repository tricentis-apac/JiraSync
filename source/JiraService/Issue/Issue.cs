using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraService.Issue
{
    public class Issue
    {
        public Issue()
        {
            fields = new IssueFields();
        }
        public string key { get; set; }
        public string self { get; set; }
        public int? id { get; set; }
        public IssueFields fields { get; set; }
    }
}
