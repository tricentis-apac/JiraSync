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

        public async Task<Issue[]> SearchAsync(string jql)
        {
            var result = await client.GetAsync($"/rest/api/latest/search?jql={jql}");
            string responseContent = result.Content.ReadAsStringAsync().Result;
            SearchResults results = JsonConvert.DeserializeObject<SearchResults>(responseContent, serializerSettings);
            return results.issues;
        }

        public async Task<Issue> GetAsync(string key)
        {
            var result = await client.GetAsync($"/rest/api/latest/issue/{key}");
            string responseContent = result.Content.ReadAsStringAsync().Result;
            Issue results = JsonConvert.DeserializeObject<Issue>(responseContent, serializerSettings);
            return results;
        }

        public async Task<bool> UpdateAsync(Issue issue)
        {
            var contentText = JsonConvert.SerializeObject(issue, serializerSettings);
            StringContent content = new StringContent(contentText,Encoding.UTF8,"application/json");
            var result = await client.PutAsync($"/rest/api/latest/issue/{issue.key}",content);
            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
                return true;
            return false;
        }
        public async Task<Issue> CreateAsync(Issue issue)
        {
            var contentText = JsonConvert.SerializeObject(issue, serializerSettings);
            StringContent content = new StringContent(contentText,Encoding.UTF8,"application/json");
            var result = await client.PostAsync($"/rest/api/latest/issue", content);
            string responseContent = result.Content.ReadAsStringAsync().Result;
            Issue results = JsonConvert.DeserializeObject<Issue>(responseContent, serializerSettings);
            return results;
        }
    }
}
