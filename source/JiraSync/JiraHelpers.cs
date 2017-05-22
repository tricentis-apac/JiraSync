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
        //public static HttpResponseMessage Search(String username, String password, String url, String urlParameters)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(url);
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));

        //    HttpResponseMessage response = client.GetAsync(urlParameters).Result;

        //    return response;
        //}

        //public static async Task<JiraDefect> CreateIssue(String username, String password, String url, String project, String summary, String description, String issueType = "Bug")
    //    {
    //        JiraIssue jIssue = new JiraIssue();
    //        string issueEndpoint;
    //        jIssue.fields.summary = summary;
    //        jIssue.fields.description = description;
    //        jIssue.fields.project = new Project { key = project };
    //        jIssue.fields.issuetype = new IssueType { name = issueType };
    //        if (!url.Contains("/issue"))
    //        {
    //            string baseURL = url.Split(new char[] { '/' }).Take(3).Aggregate((x, y) => $"{x}/{y}");
    //            issueEndpoint = $"{baseURL}/rest/api/latest/issue";
    //        }
    //        else
    //            issueEndpoint = url;

    //        HttpClient client = new HttpClient();
    //        client.BaseAddress = new Uri(issueEndpoint);
    //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));
    //        //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

    //        var serializerSettigns = new JsonSerializerSettings
    //        {
    //            NullValueHandling = NullValueHandling.Ignore
    //        };

    //        StringContent payload = new StringContent(JsonConvert.SerializeObject(jIssue, Newtonsoft.Json.Formatting.None,serializerSettigns), Encoding.UTF8, "application/json");
    //        //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");
    //        //request.Content = payload;


    //        HttpResponseMessage response = await client.PostAsync(client.BaseAddress, payload);
    //        JiraDefect defectPlaceholder = JsonConvert.DeserializeObject<JiraDefect>(response.Content.ReadAsStringAsync().Result);
    //        return defectPlaceholder;
    //    }
    }
}
