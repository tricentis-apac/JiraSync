using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync.ConfigEditor
{
    public class ConfigConstructor
    {
        public JiraService.Configuration.JiraConfig currentConfig { get; set; }
        public string[] availableAttributes { get; set; }
    }
}
