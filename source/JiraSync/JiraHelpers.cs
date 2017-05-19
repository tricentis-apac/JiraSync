using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JiraSync
{
    public class JiraHelpers
    {
        public static HttpResponseMessage Search(String username, String password, String url, String urlParameters)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;

            return response;
        }

        public static async Task<JiraDefect> CreateIssue(String username, String password, String url, String project, String summary, String description, String issueType = "Bug")
        {
            JiraIssue jIssue = new JiraIssue();
            jIssue.summary = summary;
            jIssue.description = description;
            jIssue.project.Add("project", project);
            jIssue.issueType.Add("name", issueType);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            StringContent payload = new StringContent(JsonConvert.SerializeObject(jIssue), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Content = payload;

            //using (HttpResponseMessage resp = await client.PostAsync(url, request).ConfigureAwait(false))

           // using (HttpResponseMessage response = await client.PostAsync(url, request).ConfigureAwait(false))


                //    HttpResponseMessage response;
                //client.SendAsync(request).ContinueWith(responseTask =>
                //    {
                //        response = responseTask.Result;
                //    });


                return new JiraDefect("", "", "");
        }

        public class JiraDefect
        {
            public String ID;
            public String Key;
            public String URL;

            public JiraDefect(String id, String key, String url)
            {
                ID = id;
                Key = key;
                URL = url;
            }
        }

        public class JiraIssue
        {
            public Dictionary<String, String> project = new Dictionary<string, string>();
            public Dictionary<String, String> issueType = new Dictionary<string, string>();

            public String summary;
            public String description;
        }
    }
}
