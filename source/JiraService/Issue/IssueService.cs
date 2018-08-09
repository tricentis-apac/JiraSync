﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Jira.Mock;

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


#if DEVELOPMENT
            string responseContent = new MockJiraService().GetIssues();
#else
            var result = await client.GetAsync($"/rest/api/latest/search?jql={jql}");
            string responseContent = result.Content.ReadAsStringAsync().Result;
#endif
            SearchResults results = JsonConvert.DeserializeObject<SearchResults>(responseContent, serializerSettings);
            return results.issues;
        }

        public Issue[] Search(string jql)
        {


#if DEVELOPMENT
            string responseContent = new MockJiraService().GetIssues();
#else
            var result = client.GetAsync($"/rest/api/latest/search?jql={jql}").Result;
            string responseContent = result.Content.ReadAsStringAsync().Result;
#endif
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error fetching content. Error code: {result.StatusCode}, Reason: {result.ReasonPhrase}");
            SearchResults results = JsonConvert.DeserializeObject<SearchResults>(responseContent, serializerSettings);
            return results.issues;
        }

        public async Task<Issue> GetAsync(string key)
        {
#if DEVELOPMENT
            string responseContent = new MockJiraService().GetIssue();
#else
            var result = await client.GetAsync($"/rest/api/latest/issue/{key}");
            string responseContent = result.Content.ReadAsStringAsync().Result;

            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error fetching content. Error code: {result.StatusCode}, Reason: {result.ReasonPhrase}");
#endif
            Issue results = JsonConvert.DeserializeObject<Issue>(responseContent, serializerSettings);
            //JsonConvert.DeserializeObject(,,new JsonConverter())

            return results;
            
        }

        public async Task<bool> UpdateAsync(Issue issue)
        {
            var contentText = JsonConvert.SerializeObject(issue, serializerSettings);
#if DEVELOPMENT
            return true;
#else
            StringContent content = new StringContent(contentText,Encoding.UTF8,"application/json");
            var result = await client.PutAsync($"/rest/api/latest/issue/{issue.key}",content);
            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
                return true;

            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error fetching content. Error code: {result.StatusCode}, Reason: {result.ReasonPhrase}");
            return false;
#endif

        }
        public async Task<Issue> CreateAsync(Issue issue)
        {
            var contentText = JsonConvert.SerializeObject(issue, serializerSettings);
            StringContent content = new StringContent(contentText,Encoding.UTF8,"application/json");
#if DEVELOPMENT
            string responseContent = new MockJiraService().PostIssue();
#else
            var result = await client.PostAsync($"/rest/api/latest/issue", content);
            string responseContent = result.Content.ReadAsStringAsync().Result;

            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error fetching content. Error code: {result.StatusCode}, Reason: {result.ReasonPhrase}");
#endif
            Issue results = JsonConvert.DeserializeObject<Issue>(responseContent, serializerSettings);

            return results;
        }
    }
}
