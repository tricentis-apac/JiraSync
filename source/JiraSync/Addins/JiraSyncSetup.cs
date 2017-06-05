using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects.Objects;

namespace JiraSync.Addins
{

    public class JiraSyncSetupAddOn : TCAddOn
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
            ToscaHelpers.CreateCustomProperties("Requirement", Global.JiraTicket);
            ToscaHelpers.CreateCustomProperties("Requirement", Global.JiraLastSyncedAttributeName);
            ToscaHelpers.CreateCustomProperties("Requirement", Global.JiraSyncStateAttributeName);

            ToscaHelpers.CreateIssuesProperties("Issue", Global.JiraTicket);

            return null;
        }
    }

}
