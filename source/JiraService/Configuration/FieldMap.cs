using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraService.Configuration
{
    public class FieldMap
    {
        public string jiraJsonPath { get; set; }
        public string toscaField { get; set; }
        public Direction direction { get; set; }
    }
    public enum Direction
    {
        jira_to_tosca = 0,
        bidirectional = 1
    }
}
