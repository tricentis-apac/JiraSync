using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JiraService.Field
{
    public class FieldService
    {
        private HttpClient client;
        private JsonSerializerSettings serializerSettings;

        public FieldService(HttpClient client, JsonSerializerSettings serializerSettings)
        {
            this.client = client;
            this.serializerSettings = serializerSettings;
        }

        public async Task<Field[]> GetFieldsAsync()
        {
            var result = await client.GetAsync($"/rest/api/latest/field");
            string responseContent = result.Content.ReadAsStringAsync().Result;
            Field[] results = JsonConvert.DeserializeObject<Field[]>(responseContent, serializerSettings);
            return results;

        }
    }
}
