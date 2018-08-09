using JiraService.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects.Objects;
using Tricentis.TCCore.CredentialManager;

namespace JiraSync.Addins
{
    public class RefreshDefects : TCAddOnTask
    {
        public override string Name => "Refresh Defect(s)";

        public override Type ApplicableType => typeof(TCObject);

        public override bool IsTaskPossible(TCObject obj)
        {
            try
            {
                if (obj is TCFolder)
                {
                    TCFolder f = (TCFolder)obj;
                    return (f.PossibleContent.Contains("Issue"));
                }
                return false;
            }
            catch
            {
                return base.IsTaskPossible(obj);
            }
        }

        public override bool RequiresChangeRights => true;

        public override TCObject Execute(TCObject objectToExecuteOn, TCAddOnTaskContext taskContext)
        {
            TCFolder f = (TCFolder)objectToExecuteOn;
            IEnumerable<Issue> childIssues = f.Search("->SUBPARTS:Issue").Cast<Issue>();

            var config = f.GetJiraConfig();
            if (config == null)
            {
                string url = taskContext.GetStringValue("Jira Instance URL: ", false);
                string project = taskContext.GetStringValue("Jira Project Key: ", false);
                config = new JiraConfig { baseURL = url, projectKey = project, fieldMaps = new List<FieldMap>() };
                f.SaveConfig(config);
            }
            string username, password;
            if (CredentialManager.Instance.Credentials.Any(x => x.BaseURL == config.baseURL))
            {
                Credential credential = CredentialManager.Instance.Credentials.First(x => x.BaseURL == config.baseURL);
                username = credential.Username;
                password = credential.Password;
            }
            else
            {
                username = taskContext.GetStringValue("Jira Username", false);
                password = taskContext.GetStringValue("Jira Password", true);
                CredentialManager.Instance.StoreOrUpdateCredential(new Credential
                {
                    BaseURL = config.baseURL,
                    Description = "Created by Jira Config",
                    Username = username,
                    Password = password
                });
            }

            var jira = new JiraService.Jira(config.baseURL, username, password);
            var issueService = jira.GetIssueService();
            foreach (var issue in childIssues)
            {

                string storedIssueKey = string.Empty;
                try
                {
                    storedIssueKey = issue.GetAttributeValue(Global.JiraTicket);
                }
                catch (Exception)
                {
                    throw new Exception("Please prepare project for integration (available from context menu at project level) and then try again");
                }
                if (!string.IsNullOrEmpty(storedIssueKey))
                {
                    var jiraIssue = issueService.GetAsync(storedIssueKey).Result;
                    issue.State = jiraIssue.fields.status.name;
                    issue.Name = jiraIssue.fields.summary;
                }
                else //No existing Jira issue exists
                {
                    string description = issue.Description;
                    if (issue.Links.Any())
                    {
                        try
                        {
                            var executionLog = issue.Links.First().ExecutionTestCaseLog;
                            string executionTableHeader = $"||Step||Result||Description||Duration(sec)";
                            string executionTable = null;
                            foreach (ExecutionXTestStepLog logEntry in executionLog.ExecutionSubLogs)
                            {
                                string stepDesc = logEntry.AggregatedDescription.Replace('{', ' ').Replace('}', ' ').Replace('|', ' ').Trim();
                                
                                if(logEntry.TestStepValueLogsInRightOrder.Count() > 0)
                                {

                                }
                                string entry = $"|{logEntry.DisplayedName} |{(logEntry.Result == ExecutionResult.Passed ? "{color:#14892c}" : "{color:#d04437}") + logEntry.Result + "{color}"} |{stepDesc}|{Math.Round(logEntry.Duration / 1000,2)}s|";
                                if (executionTable == null)
                                    executionTable = entry;
                                else
                                    executionTable += "\r\n" + entry;
                            }
                            description = $"*TEST*: {executionLog.Name}\r\n*Description*:\r\n{executionTableHeader}\r\n{executionTable}";
                        }
                        catch (Exception)
                        {
                            description = issue.Description;
                        }
                        
                    }
                    var newIssue = new JiraService.Issue.Issue
                    {
                        fields = new JiraService.Issue.IssueFields
                        {
                            summary = issue.Name,
                            description = description,
                            //Create other fields here
                            project = new JiraService.Issue.Field.ProjectField { key = config.projectKey },
                            issuetype = new JiraService.Issue.Field.IssueTypeField { name = "Bug" }
                        }
                    };
                    foreach (var defaultValue in config.defaultValues)
                    {
                        newIssue.SetValueByPath(defaultValue.jiraJsonPath, defaultValue.defaultValue);
                    }
                    JiraService.Issue.Issue createdIssue = issueService.CreateAsync(newIssue).Result;
                    createdIssue = issueService.GetAsync(createdIssue.key).Result; //The created issue only contains a shell, no fields
                    issue.SetAttibuteValue(Global.JiraTicket, createdIssue.key);
                    issue.State = createdIssue.fields.status.name;
                }

            }
            return objectToExecuteOn;
        }
    }
}
