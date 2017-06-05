using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraService.Issue
{
    public class Issue
    {
        private JObject _jsonRepresentation;
        private Dictionary<string, string> _modifications;
        public Issue()
        {
            fields = new IssueFields();
        }
        internal void SetIssueFields(JObject issue)
        {
            this._jsonRepresentation = issue;
        }
        public string GetValueByPath(string jsonPath)
        {
            var token = _jsonRepresentation.SelectToken(jsonPath);
            var x = new Newtonsoft.Json.Linq.JTokenWriter();
            try
            {
                return token.Value<string>();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public void SetValueByPath(string jsonPath, string value)
        {
            if (_modifications == null)
                _modifications = new Dictionary<string, string>();
            if (_modifications.ContainsKey(jsonPath))
                _modifications[jsonPath] = value;
            else
                _modifications.Add(jsonPath, value);
        }

        public string SerializeObject(JsonSerializerSettings settings)
        {
            string thisObjectAsJson = JsonConvert.SerializeObject(this,settings);
            JObject jObj = JObject.Parse(thisObjectAsJson);
            if (_modifications == null)
                _modifications = new Dictionary<string, string>();
            foreach (var modifier in _modifications)
            {
                //Look forwards until we get to the nearest JToken
                string[] nodes = modifier.Key.Replace("$.", "").Split(new char[] { '.' });
                int i;
                JToken lastExistingRoot = null;
                for (i = 0; i < nodes.Length; i++)
                {
                    if (lastExistingRoot == null)
                        lastExistingRoot = jObj;
                    if (lastExistingRoot[nodes[i]] == null)
                        break;
                    lastExistingRoot = lastExistingRoot[nodes[i]];
                }

                if (i == nodes.Length)
                {
                    lastExistingRoot.Replace(modifier.Value);
                    continue;
                }
                //Handle replacing existing value
                //lastExistingRoot is Fields
                if(i == 0)
                {
                    if (jObj[nodes[0]] == null)
                        jObj.Add(nodes[0], modifier.Value);
                    else
                        jObj[nodes[0]].Replace(modifier.Value);
                    continue;
                }

                JProperty prop = new JProperty(nodes[nodes.Length - 1], modifier.Value);
                JToken currentToken = prop;
                for (int j = Math.Max(nodes.Length - 2,0); i > j; i--)
                {
                    var newToken = new JProperty(nodes[i], new JObject(currentToken));
                    currentToken = newToken;
                }
                ((JContainer)lastExistingRoot).Add(new JProperty(nodes[i], new JObject(currentToken)));
            }
            return jObj.ToString();
        }


        public string key { get; set; }
        public string self { get; set; }
        public int? id { get; set; }
        public IssueFields fields { get; set; }

    }
}
