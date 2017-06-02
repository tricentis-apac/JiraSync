using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync.Configuration
{
    public class JiraConfig
    {
        public string baseURL { get; set; }
        public string jqlFilter { get; set; }
        public string projectKey { get; set; }
        public List<FieldMap> fieldMaps { get; set; }
    }
}
