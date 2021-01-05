using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SendAnywhere
{
    public class SendClass
    {
        private int key = -1;

        private List<string> filePaths = new List<string>();
        private Dictionary<string, List<Dictionary<string, object>>> requests_payload = new Dictionary<string, List<Dictionary<string, object>>>();
        private Dictionary<string, string> file_names = new Dictionary<string, string>();

        private const string default_device_key = "MY_DEFAULT_DEVICE_KEY";
        private string device_key = default_device_key;

        private string session_start_link = string.Empty;

        public SendClass(string[] filePaths)
        {
            Initialize(filePaths, default_device_key);
        }

        public SendClass(string[] filePaths, string deviceKey)
        {
            Initialize(filePaths, deviceKey);
        }

        private void Initialize(string[] filePaths, string deviceKey)
        {
            this.filePaths.AddRange(filePaths);
            this.device_key = deviceKey;

            foreach (string x in this.filePaths)
            {
                Dictionary<string, object> info = new Dictionary<string, object>();
                info.Add("name", Path.GetFileName(x));
                info.Add("size", GetFileSize(x));

                if (!this.requests_payload.ContainsKey("file"))
                {
                    this.requests_payload.Add("file", new List<Dictionary<string, object>>());
                }
                this.requests_payload["file"].Add(info);

                this.file_names.Add(Path.GetFileName(x), x);
            }
        }

        public async Task<int> SendAsyncWithKey()
        {
            await SendAsync();
            return this.key;
        }

        public async Task<string> SendAsyncWithKeyToString()
        {
            await SendAsync();
            return GetKeyToString();
        }

        private async Task<Dictionary<string, string>> SendAsync()
        {
            string payload_json = DictionaryToJson(this.requests_payload);
            HttpClient httpClient = new HttpClient();

            StringContent payload = new StringContent(payload_json, Encoding.UTF8, "application/json");
            UriBuilder baseUri = new UriBuilder("https://send-anywhere.com/web/key");
            baseUri.Query = "device_key=" + this.device_key;

            HttpResponseMessage req = await httpClient.PostAsync(baseUri.Uri, payload);
            string json = await req.Content.ReadAsStringAsync();
            Dictionary<string, string> dict = JsonToDictionary<string, string>(json);

            string link = dict["weblink"];

            this.key = Convert.ToInt32(dict["key"]);
            this.session_start_link = link.Substring(0, link.IndexOf("/api/") + 5) + "session_start/" + GetKeyToString();

            return dict;
        }

        public async Task FetchAsync()
        {
            try
            {
                string session_link = ReplaceFirst(this.session_start_link, "/session_start/", "/session/");
                string session_finish_link = ReplaceFirst(this.session_start_link, "/session_start/", "/session_finish/");
                string data_upload_link = session_link.Substring(0, session_link.IndexOf("/session/") + 9) + "file/";

                string payload_json = DictionaryToJson(this.requests_payload);
                HttpClient httpClient = new HttpClient();

                StringContent payload = new StringContent(payload_json, Encoding.UTF8, "application/json");
                UriBuilder baseUri = new UriBuilder(session_start_link);
                baseUri.Query = "device_key=" + this.device_key;

                HttpResponseMessage req = await httpClient.PostAsync(baseUri.Uri, payload);
                string json = await req.Content.ReadAsStringAsync();
                var key_data = JsonToDictionary<string, object>(json);

                foreach (var x in (Newtonsoft.Json.Linq.JArray)key_data["file"])
                {
                    Dictionary<string, string> file = new Dictionary<string, string>();
                    file.Add("key", x.ToObject<Dictionary<string, object>>()["key"].ToString());
                    file.Add("name", x.ToObject<Dictionary<string, object>>()["name"].ToString());
                    file.Add("path", this.file_names[file["name"]]);
                    byte[] file_data = File.ReadAllBytes(file["path"]);

                    baseUri = new UriBuilder(data_upload_link + file["key"]);
                    baseUri.Query = "device_key=" + this.device_key + "&offset=0";

                    MultipartFormDataContent fileContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    fileContent.Add(new StreamContent(new MemoryStream(file_data)), file["name"], file["name"]);

                    await httpClient.PostAsync(baseUri.Uri, fileContent);
                }

                //session end
                baseUri = new UriBuilder(session_link);
                baseUri.Query = "device_key=" + this.device_key + "&mode=status&_=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                await httpClient.GetAsync(baseUri.Uri);

                baseUri = new UriBuilder(session_finish_link);
                baseUri.Query = "device_key=" + this.device_key + "&mode=status&_=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                await httpClient.GetAsync(baseUri.Uri);
            }
            catch
            {
                return;
            }
        }

        public string GetKeyToString()
        {
            return this.key.ToString("D6");
        }

        /// <summary>
        /// Convert Dictionary Type to Json Type.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private string DictionaryToJson<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            return JsonConvert.SerializeObject(dict);
        }

        /// <summary>
        /// Convert Json Type To Dictionary Type
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private Dictionary<TKey, TValue> JsonToDictionary<TKey, TValue>(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }
    }
}
