using JiraService.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync.ConfigEditor.ViewModels
{
    public class ConfigViewModel : ViewModelBase
    {
        private JiraConfig config;
        public ConfigViewModel() { this.config = new JiraConfig(); }
        public ConfigViewModel(JiraService.Configuration.JiraConfig config) {
            this.config = config;
        }

        public string BaseURL {
            get { return config.baseURL; }
            set
            {
                if (config.baseURL == value)
                    return;
                config.baseURL = value;
                base.NotifyPropertyChanged(nameof(BaseURL));
            }
        }

        public string JQLFilter
        {
            get
            {
                return config.jqlFilter;
            }
            set
            {
                if (config.jqlFilter == value)
                    return;
                config.jqlFilter = value;
                base.NotifyPropertyChanged(nameof(JQLFilter));
            }
        }

        public string ProjectKey
        {
            get
            {
                return config.projectKey;
            }
            set
            {
                if (config.projectKey == value)
                    return;
                config.projectKey = value;
                base.NotifyPropertyChanged(nameof(ProjectKey));
            }
        }

        public JiraConfig Config { get { return config; } }

    }
}
