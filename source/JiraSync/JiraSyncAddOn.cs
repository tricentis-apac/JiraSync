using JiraSync.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects.Objects;
using static JiraSync.JiraHelpers;

namespace JiraSync
{
    public class JiraSyncAddOn : TCAddOn
    {
        public override string DisplayedName => "Jira Sync";

        public override string UniqueName => "JiraSync";
    }

    public class JiraSyncSetup : TCAddOnTask
    {
        public override string Name => "Setup Tosca Defintions";

        public override Type ApplicableType => typeof(TCProject);

        public override bool RequiresChangeRights => true;

        public override TCObject Execute(TCObject objectToExecuteOn, TCAddOnTaskContext taskContext)
        {
            // Create custom properties on Requirements and RequirementSets.
            // This is to be used to search when updating or knowing which ones have been created by this addon.
            ToscaHelpers.CreateCustomProperties("Requirement", Global.JiraTicketAttributeName);
            ToscaHelpers.CreateCustomProperties("Requirement", Global.JiraLastSyncedAttributeName);
            ToscaHelpers.CreateCustomProperties("Requirement", Global.JiraSyncStateAttributeName);

            ToscaHelpers.CreateIssuesProperties("Issue", Global.JiraDefectID);
            ToscaHelpers.CreateIssuesProperties("Issue", Global.JiraDefectKey);

            return null;
        }
    }

    public class JiraSyncRequirementSetSetup : TCAddOnTask
    {
        public override string Name => "Configure for Jira Sync";

        public override Type ApplicableType => typeof(RequirementSet);

        public override bool RequiresChangeRights => true;

        public override TCObject Execute(TCObject objectToExecuteOn, TCAddOnTaskContext taskContext)
        {
            String baseURL = taskContext.GetStringValue("Base Jira URL", false);
            String jql = taskContext.GetStringValue("Enter JQL", false);

            // Provide default values as hints when left empty
            if (baseURL == "")
                baseURL = "https://subdomain.atlassian.net/rest/api/2/search";


            objectToExecuteOn.SetAttibuteValue(Global.JiraBaseURL, baseURL);
            objectToExecuteOn.SetAttibuteValue(Global.JiraBaseJQL, jql);

            return null;
        }
    }

    public class JiraRequirements : TCAddOnTask
    {
        public override string Name => "Update Requirements";

        public override Type ApplicableType => typeof(RequirementSet);

        public override bool RequiresChangeRights => true;

        public override TCObject Execute(TCObject requirementSet, TCAddOnTaskContext taskContext)
        {
            String username = taskContext.GetStringValue("Jira Username", false);
            String password = taskContext.GetStringValue("Jira Password", true);
            RequirementSet rs = (RequirementSet)requirementSet;
            JiraConfig config = rs.GetJiraConfig();
            if (config == null)
            {
                string url = taskContext.GetStringValue("Jira Instance URL: ", false);
                string jqlValue = taskContext.GetStringValue("JQL Filter for requirements: ", false);
                config = new JiraConfig
                {
                    baseURL = url,
                    jqlFilter = jqlValue,
                    fieldMaps = new List<FieldMap>()
                {
                    new FieldMap {direction = Direction.jira_to_tosca, jiraJsonPath="$.fields.summary", toscaField="Name" }
                }
                };
                rs.SaveConfig(config);
            }
            var jira = new JiraService.Jira(config.baseURL, username, password);
            var issueService = jira.GetIssueService();
            String startTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string jql = config.jqlFilter;
            JiraService.Issue.Issue[] issues = null;
            Task<JiraService.Issue.Issue[]> issueTask = null;
            try
            {
                issueTask = issueService.SearchAsync(jql);
                while (!issueTask.IsCompleted)
                {
                    taskContext.ShowStatusInfo($"Gettign issues for JQL: {jql}");
                    System.Threading.Thread.Sleep(100);
                }
                //order the issues so that subtasks are not created before parent tasks
                issues = issueTask.Result.OrderBy(x => x.id).ToArray();
                taskContext.ShowStatusInfo("Creating Requirements");
            }
            catch (Exception e)
            {
                taskContext.ShowStatusInfo($"Error synchronising: {e.Message}");
            }

            if (issues != null)
            {
                foreach (var issue in issues)
                {
                    CreateOrUpdateRequirement(rs, config, issue);
                }

                // Prompt status
                taskContext.ShowMessageBox("Jira Sync", issues.Length.ToString() + " requirements have been synchronised.");
            }
            return null;
        }

        private Requirement CreateOrUpdateRequirement(RequirementSet rs, JiraConfig config, JiraService.Issue.Issue issue)
        {
            Requirement req = FindRequirementForIssue(rs, issue.key);
            if (req == null)
                req = rs.CreateRequirement();
            //Move requirement to parent
            if (issue.fields.parent != null)
            {
                Requirement parent = FindRequirementForIssue(rs, issue.fields.parent.key);
                if (parent != null)
                    parent.Move(req);
            }
            try
            {
                req.SetAttibuteValue(Global.JiraTicketAttributeName, issue.key);
            }
            catch (Exception)
            {

                throw new Exception("Please prepare project for integration (available from context menu at project level) and then try again");
            }
            //req.SetAttibuteValue(Global.JiraDefectID, issue.id);
            foreach (var fieldMap in config.fieldMaps)
            {
                string jiraValue = issue.fields.GetValueByPath(fieldMap.jiraJsonPath);
                try
                {
                    req.SetAttibuteValue(fieldMap.toscaField, jiraValue);
                }
                catch (Exception)
                {
                    //NOM NOM NOM: Tasty tasty exceptions
                }
            }
            return req;
        }


        private Requirement FindRequirementForIssue(RequirementSet rs, string issueKey)
        {
            Requirement req = rs.Search($"=>SUBPARTS:Requirement[{Global.JiraTicketAttributeName}==\"{issueKey}\"]").FirstOrDefault() as Requirement;
            return req;
        }
    }

    public class JiraSubmitDefect : TCAddOnTask
    {
        public override string Name => "Submit Defect";

        public override Type ApplicableType => typeof(Issue);

        public override bool RequiresChangeRights => true;

        public override TCObject Execute(TCObject objectToExecuteOn, TCAddOnTaskContext taskContext)
        {
            String username = taskContext.GetStringValue("Jira Username", false);
            String password = taskContext.GetStringValue("Jira Password", true);

            #region Get URL

            String url = "";
            List<TCFolder> folders = objectToExecuteOn.Search("=>SUPERPART:TCFolder").Cast<TCFolder>().ToList();
            foreach (TCFolder folder in folders.OrderByDescending(f => f.NodePath))
            {
                var config = folder.GetJiraConfig();
                if (config != null)
                {
                    url = config.baseURL;
                    break;
                }
            }
            // String url = "https://subdomain.atlassian.net/rest/api/2/issue/";

            #endregion

            //JiraDefect jd = CreateIssue(username, password, url, "Tosca Project", "Tosca Summary", "Tosca description", "Bug");

            //objectToExecuteOn.SetAttibuteValue(Global.JiraDefectID, jd.ID);
            //objectToExecuteOn.SetAttibuteValue(Global.JiraDefectKey, jd.Key);
            //objectToExecuteOn.SetAttibuteValue(Global.JiraDefectURL, jd.URL);

            return null;
        }

    }

    /// <summary>
    /// This should work on both Issue folders and individual issues
    /// </summary>
    public class JiraRefreshDefects : TCAddOnTask
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
            IEnumerable<Issue> childIssues = f.Items.Cast<Issue>();
            String username = taskContext.GetStringValue("Jira Username", false);
            String password = taskContext.GetStringValue("Jira Password", true);
            var config = f.GetJiraConfig();
            if (config == null)
            {
                string url = taskContext.GetStringValue("Jira Instance URL: ", false);
                string project = taskContext.GetStringValue("Jira Project Key: ", false);
                config = new JiraConfig { baseURL = url, projectKey = project, fieldMaps = new List<FieldMap>() };
                f.SaveConfig(config);
            }
            var jira = new JiraService.Jira(config.baseURL, username, password);
            var issueService = jira.GetIssueService();
            foreach (var issue in childIssues)
            {

                string storedIssueKey = string.Empty;
                try
                {
                    storedIssueKey = issue.GetAttributeValue(Global.JiraDefectKey);
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
                        var executionLog = issue.Links.First().ExecutionTestCaseLog;
                        description = $"TEST: {executionLog.Name}\r\n{executionLog.AggregatedDescription}";
                    }
                    JiraService.Issue.Issue createdIssue = issueService.CreateAsync(new JiraService.Issue.Issue
                    {
                        fields = new JiraService.Issue.IssueFields
                        {
                            summary = issue.Name,
                            description = description,
                            //Create other fields here
                            project = new JiraService.Issue.Field.ProjectField { key = config.projectKey },
                            issuetype = new JiraService.Issue.Field.IssueTypeField { name = "Bug" }
                        }
                    }).Result;
                    createdIssue = issueService.GetAsync(createdIssue.key).Result; //The created issue only contains a shell, no fields
                    issue.SetAttibuteValue(Global.JiraDefectKey, createdIssue.key);
                    issue.State = createdIssue.fields.status.name;
                }

            }
            return objectToExecuteOn;
        }
    }
}
