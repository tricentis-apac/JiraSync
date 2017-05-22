using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraService.Field
{
    public class Field
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool? custom { get; set; }
        public bool? orderable { get; set; }
        public bool? navigable { get; set; }
        public bool? searchable { get; set; }
        public List<string> clauseNames { get; set; }
    }
}
