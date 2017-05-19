using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

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

            String startTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            HttpResponseMessage response = JiraHelpers.Search(username, password, requirementSet.GetAttributeValue(Global.JiraBaseURL), "?jql=" + requirementSet.GetAttributeValue(Global.JiraBaseJQL));
            if (response.IsSuccessStatusCode)
            {
                dynamic resp = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                foreach (dynamic issue in resp.issues)
                {
                    // Find if it exists already
                    Requirement reqObj = ToscaHelpers.FindRequirementByJiraProperty(requirementSet, Convert.ToString(issue.key));

                    #region Jira Parent Object
                    String jiraParent = "";
                    Requirement reqParentObj = null;

                    JObject obj = issue.fields;
                    String key = Convert.ToString(issue.key);
                    String summary = obj.Property("summary").Value.ToString();

                    // Check if it is a child of something
                    if (obj.Property("parent") != null)
                    {
                        jiraParent = Convert.ToString(issue.fields.parent.key);
                        reqParentObj = ToscaHelpers.FindRequirementByJiraProperty(requirementSet, jiraParent);
                    }
                    #endregion

                    // Meant to have a parent requirement or in root
                    if (String.IsNullOrWhiteSpace(jiraParent))
                    {   // Belongs in root

                        if (reqObj == null)
                            CreateRequirement((RequirementSet)requirementSet, key, summary, startTime);
                        else
                            UpdateRequirement(reqObj, key, summary, startTime, requirementSet);
                    }
                    else
                    {   // Belongs to a requirement
                        if (reqParentObj == null)
                        {
                            //Create temporary parent first
                            Requirement parentReq = CreateRequirement((RequirementSet)requirementSet, jiraParent, jiraParent, startTime);

                            //Create/Update the requirement in temporary/existing parent
                            if (reqObj == null)
                                CreateRequirement(parentReq, key, summary, startTime);
                            else
                                UpdateRequirement(reqObj, key, summary, startTime, parentReq);
                        }
                        else
                        {
                            // Create sub requirement in requirement
                            if (reqObj == null)
                                CreateRequirement(reqParentObj, key, summary, startTime);
                            else
                                UpdateRequirement(reqObj, key, summary, startTime, reqParentObj);
                        }
                    }
                }

                JArray issues = resp.issues;
                taskContext.ShowMessageBox("Jira Sync", issues.Count.ToString() + " requirements have been synchronised.");
            }
            else
            {
                // Invalid response from Jira.
                // Handle this somehow.

                taskContext.ShowErrorMessage("Jira Sync", response.StatusCode + Environment.NewLine + response.Content.ReadAsStringAsync());
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
            throw new NotImplementedException();
        }
    }
}
