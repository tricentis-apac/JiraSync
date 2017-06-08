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
    public class UpdateCredentials : TCAddOnTask
    {
        public override string Name => "Update Credentials";

        public override Type ApplicableType => typeof(TCObject);

        public override bool IsTaskPossible(TCObject obj)
        {
            return true;
        }

        public override bool RequiresChangeRights => false;

        public override TCObject Execute(TCObject objectToExecuteOn, TCAddOnTaskContext taskContext)
        {
            List<string> credentialUrls = CredentialManager.Instance.Credentials.Select(x => x.BaseURL).ToList();
            string result = taskContext.GetStringSelection("Select the URL which you want to enter new credentials for:", credentialUrls);
            if(CredentialManager.Instance.Credentials.Any(x=>x.BaseURL == result))
            {
                string username = taskContext.GetStringValue("Username: ", false);
                string password = taskContext.GetStringValue("Password: ", true);
                CredentialManager.Instance.StoreOrUpdateCredential(new Credential
                {
                    BaseURL = result,
                    Username = username,
                    Password = password
                });
            }
            return objectToExecuteOn;
        }
    }
}
