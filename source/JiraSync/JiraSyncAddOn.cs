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

            ToscaHelpers.CreateCustomProperties("RequirementSet", Global.JiraBaseURL);
            ToscaHelpers.CreateCustomProperties("RequirementSet", Global.JiraBaseJQL);

            ToscaHelpers.CreateIssuesProperties("Issue", Global.JiraDefectID);
            ToscaHelpers.CreateIssuesProperties("Issue", Global.JiraDefectKey);

            ToscaHelpers.CreateIssuesProperties("Folder", Global.JiraDefectURL);
            ToscaHelpers.CreateIssuesProperties("Folder", Global.JiraBaseJQL);
            ToscaHelpers.CreateIssuesProperties("Folder", Global.JiraDefectProject);

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

            if (jql == "")
                jql = "priority=medium";

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
            var jira = new JiraService.Jira(requirementSet.GetAttributeValue(Global.JiraBaseURL), username, password);
            var issueService = jira.GetIssueService();
            String startTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string jql = requirementSet.GetAttributeValue(Global.JiraBaseJQL);
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
            }
            catch (Exception e)
            {
                taskContext.ShowStatusInfo($"Error synchronising: {e.Message}");
            }

            if (issues != null)
            {
                foreach (var issue in issues)
                {
                    // Find if it exists already

                    #region Jira Parent Object
                    Requirement reqObj = ToscaHelpers.RequirementHelpers.FindRequirementByJiraProperty(requirementSet, Convert.ToString(issue.key));
                    JiraService.Issue.Issue parent = issue.fields.parent;
                    Requirement reqParentObj = null;
                    // Check if it is a child of something
                    if (parent != null)
                        reqParentObj = ToscaHelpers.RequirementHelpers.FindRequirementByJiraProperty(requirementSet, parent.key);
                    #endregion

                    // Meant to have a parent requirement or in root
                    if (parent == null)
                    {   // Belongs in root
                        if (reqParentObj == null)
                            CreateRequirement((RequirementSet)requirementSet, issue.key, issue.fields.summary, startTime);
                        else
                            UpdateRequirement(reqObj, issue.key, issue.fields.summary, startTime, requirementSet);
                    }
                    else
                    {   // Belongs to a requirement
                        if (reqParentObj == null)
                        {
                            //Create temporary parent first
                            Requirement parentReq = CreateRequirement((RequirementSet)requirementSet, parent.key, parent.key, startTime);

                            //Create/Update the requirement in temporary/existing parent
                            if (reqObj == null)
                                CreateRequirement(parentReq, issue.key, issue.fields.summary, startTime);
                            else
                                UpdateRequirement(reqObj, issue.key, issue.fields.summary, startTime, parentReq);
                        }
                        else
                        {
                            // Create sub requirement in requirement
                            if (reqObj == null)
                                CreateRequirement(reqParentObj, issue.key, issue.fields.summary, startTime);
                            else
                                UpdateRequirement(reqObj, issue.key, issue.fields.summary, startTime, reqParentObj);
                        }
                    }
                }

                // Reset state for those not updated
                foreach (TCObject obj in requirementSet.Search("=>SUBPARTS:Requirement"))
                {
                    if (obj.GetAttributeValue(Global.JiraLastSyncedAttributeName) != startTime)
                    {
                        obj.SetAttibuteValue(Global.JiraSyncStateAttributeName, Global.JiraSyncStates.NotUpdated);
                    }
                }

                // Create Virtual Folder if required
                TCObject rsParent = requirementSet.Search("->SUPERPART").First();
                TCVirtualFolder vf = ToscaHelpers.CreateVirtualFolder(rsParent, requirementSet.DisplayedName + " - Not Synced", "->SUPERPART=>SUBPARTS:RequirementSet[Name==\"JIR Project\"]=>SUBPARTS:Requirement[JiraSyncState!=\"" + Global.JiraSyncStates.Updated + "\"]");
                vf.RefreshVirtualFolder(); // Force refresh to show. Otherwise, results are sometimes cached from last time it was refreshed.

                // Prompt status
                taskContext.ShowMessageBox("Jira Sync", issues.Length.ToString() + " requirements have been synchronised.");
            }
            return null;
        }

        private Requirement UpdateRequirement(Requirement req, String jiraTicketNumber, String name, String syncDateTime, TCObject parent)
        {
            Dictionary<String, String> props = new Dictionary<string, string>();
            props.Add(Global.JiraLastSyncedAttributeName, syncDateTime);
            props.Add(Global.JiraSyncStateAttributeName, Global.JiraSyncStates.Updated.ToString());

            ToscaHelpers.RequirementHelpers.UpdateRequirement(req, jiraTicketNumber + "-" + name, props);

            #region Move object to new parent
            TCObject reqCurrentParent = req.Search("->SUPERPART").First();
            if (reqCurrentParent.UniqueId != parent.UniqueId)
            {
                //req.Move(parent);
                parent.Move(req);
            }
            #endregion

            return req;
        }

        private Requirement CreateRequirement(RequirementSet reqSetParent, String jiraTicketNumber, String name, String syncDateTime)
        {
            Dictionary<String, String> props = new Dictionary<string, string>();
            props.Add(Global.JiraLastSyncedAttributeName, syncDateTime);
            props.Add(Global.JiraTicketAttributeName, jiraTicketNumber);
            props.Add(Global.JiraSyncStateAttributeName, Global.JiraSyncStates.Updated.ToString());

            Requirement r = ToscaHelpers.RequirementHelpers.CreateRequirement(reqSetParent, jiraTicketNumber + "-" + name, props);
            return r;
        }

        private Requirement CreateRequirement(Requirement reqParent, String jiraTicketNumber, String name, String syncDateTime)
        {
            Dictionary<String, String> props = new Dictionary<string, string>();
            props.Add(Global.JiraLastSyncedAttributeName, syncDateTime);
            props.Add(Global.JiraTicketAttributeName, jiraTicketNumber);
            props.Add(Global.JiraSyncStateAttributeName, Global.JiraSyncStates.Updated.ToString());

            Requirement r = ToscaHelpers.RequirementHelpers.CreateRequirement(reqParent, jiraTicketNumber + "-" + name, props);
            return r;
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
            List<TCObject> folders = objectToExecuteOn.Search("=>SUPERPART:TCFolder");
            foreach (TCObject folder in folders.OrderByDescending(f => f.NodePath))
            {
                if (folder.GetPropertyNames().Contains(Global.JiraDefectURL))
                {
                    url = folder.GetPropertyValue(Global.JiraDefectURL);
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
                    
                    return (f.PossibleContent.Contains("Issue") 
                        && !string.IsNullOrEmpty(f.GetAttributeValue(Global.JiraDefectURL)));
                }

                return (obj is Issue);
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
            var jira = new JiraService.Jira(f.GetAttributeValue(Global.JiraDefectURL), username, password);
            var issueService = jira.GetIssueService();
            foreach (var issue in childIssues)
            {
                string storedIssueKey = issue.GetAttributeValue(Global.JiraDefectKey);
                if (!string.IsNullOrEmpty(storedIssueKey))
                {
                    var jiraIssue = issueService.GetAsync(storedIssueKey).Result;
                    issue.State = jiraIssue.fields.status.name;
                    issue.Name = jiraIssue.fields.summary;
                }
                else //No existing Jira issue exists
                {
                    string description = issue.Description;
                    if(issue.Links.Any())
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
                            project = new JiraService.Issue.Field.ProjectField { key = f.GetAttributeValue(Global.JiraDefectProject) },
                            issuetype = new JiraService.Issue.Field.IssueTypeField { name="Bug"}
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
