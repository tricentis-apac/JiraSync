using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync.Configuration
{
    public class JiraConfig
    {
        public JiraConfig()
        {
            this.fieldMaps = new List<FieldMap>();
            this.defaultValues = new List<FieldDefault>();
        }
        public string baseURL { get; set; }
        public string jqlFilter { get; set; }
        public string projectKey { get; set; }
        public List<FieldMap> fieldMaps { get; set; }
        public List<FieldDefault> defaultValues { get; set; }
    }
}
