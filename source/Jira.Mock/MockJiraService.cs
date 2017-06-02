using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira.Mock
{
    public class MockJiraService
    {
        public string GetIssues()
        {
            return File.ReadAllText("./Responses/search.json");
        }
        public string GetIssue()
        {
            return File.ReadAllText("./Responses/issue.json");
        }
        public string GetFields()
        {
            return File.ReadAllText("./Responses/field.json");
        }
        public string PostIssue()
        {
            return File.ReadAllText("./Responses/issuePost.json");
        }
    }
}
