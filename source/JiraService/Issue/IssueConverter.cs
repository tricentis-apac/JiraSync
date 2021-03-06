﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraService.Issue
{
    public class IssueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Issue))
                return true;
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            string jSonrepresentation = obj.ToString();
            Issue issue = JsonConvert.DeserializeObject<Issue>(jSonrepresentation);
            issue.SetIssueFields(obj);
            return issue;
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Issue issue = (Issue)value;
            writer.WriteRaw(issue.SerializeObject(new JsonSerializerSettings { NullValueHandling = serializer.NullValueHandling }));
        }
    }
}
