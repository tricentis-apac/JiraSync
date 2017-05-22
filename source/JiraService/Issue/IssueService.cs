using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JiraService.Issue
{
    public class IssueService
    {
        private HttpClient client;
        private JsonSerializerSettings serializerSettings;

        public IssueService(HttpClient client, JsonSerializerSettings serializerSettings)
        {
            this.client = client;
            this.serializerSettings = serializerSettings;
        }

        public async Task<Issue[]> Search(string jql)
        {
            var result = await client.GetAsync($"/rest/api/latest/search?jql={jql}");
            string responseContent = result.Content.ReadAsStringAsync().Result;
            SearchResults results = JsonConvert.DeserializeObject<SearchResults>(responseContent, serializerSettings);
            return results.issues;
        }
    }
}
