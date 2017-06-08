using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync.ConfigEditor.ViewModels
{
    public class FieldMapViewModel : ViewModelBase
    {
        private bool _map;
        private bool _ParentLocator;

        public string JiraJSONPath { get; set; }
        public string DisplayedName { get; set; }
        public string ToscaField { get; set; }
        public string[] ToscaFields { get; set; }
        public HashSet<string> ExampleValues { get; set; }
        public bool Map
        {
            get { return _map; }
            set
            {
                if (_map == value) return;
                _map = value;
                NotifyPropertyChanged(nameof(Map));
            }
        }
        public bool ParentLocator
        {
            get { return _ParentLocator; }
            set
            {
                if (_ParentLocator == value) return;
                _ParentLocator = value;
                NotifyPropertyChanged(nameof(ParentLocator));
            }
        }
    }
}
