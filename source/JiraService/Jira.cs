using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JiraService
{
    public class Jira
    {
        private readonly HttpClient client;
        private JsonSerializerSettings serializerSettings;

        public Jira(string baseURL, string username, string password)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));
            this.client = client;
            serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };
        }

        public Issue.IssueService GetIssueService()
        {
            return new Issue.IssueService(client, serializerSettings);
        }

    }
}
