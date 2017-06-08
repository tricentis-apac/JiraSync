using JiraService.Configuration;
using JiraSync.ConfigEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects.Objects;
using Tricentis.TCCore.CredentialManager;

namespace JiraSync.Addins
{
    public class SetConfigWindow : TCAddOnTask
    {
        public override string Name => "Set Configuration";

        public override Type ApplicableType => typeof(OwnedItem);

        public override bool RequiresChangeRights => true;
        public override bool IsTaskPossible(TCObject obj)
        {
            OwnedItem oi = (OwnedItem)obj;
            var config = oi.GetJiraConfig();
            return config != null;
        }
        public override TCObject Execute(TCObject item, TCAddOnTaskContext taskContext)
        {
            var config = ((OwnedItem)item).GetJiraConfig();
            string[] props = new string[] { };
            if (item.GetType() == typeof(RequirementSet))
            {
                props = ToscaHelpers.GetPropertyNames("requirement");
            }

            if (item.GetType() == typeof(TCFolder))
            {
                var folder = item as TCFolder;
                props = ToscaHelpers.GetPropertyNames(folder.PossibleContent);
            }

            var properties = props;
            var configConstructor = new ConfigConstructor
            {
                currentConfig = config,
                availableAttributes = properties
            };

            ParameterizedThreadStart pts = new ParameterizedThreadStart(ThreadStart);
            Thread t = new Thread(ThreadStart);
            t.SetApartmentState(ApartmentState.STA);
            t.Start(configConstructor);
            t.Join();
            ((OwnedItem)item).SaveConfig(config);
            return item;
        }
        private void ThreadStart(object target)
        {

            //if (app == null)
            //    app = new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
            var configWindow = new ConfigEditor.ConfigEditor(target as ConfigConstructor);
            //app.Run(configWindow);
            configWindow.ShowDialog();
        }
    }


}
