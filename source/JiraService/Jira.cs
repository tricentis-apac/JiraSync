﻿using Newtonsoft.Json;
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
#if !DEVELOPMENT
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes(username + ":" + password)));
            this.client = client;
#endif
            serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };
            serializerSettings.Converters.Add(new Issue.IssueConverter());
        }

        public Issue.IssueService GetIssueService()
        {
            return new Issue.IssueService(client, serializerSettings);
        }
        public Field.FieldService GetFieldService()
        {
            return new Field.FieldService(client, serializerSettings);
        }
    }
}
