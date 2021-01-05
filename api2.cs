using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SendAnywhere
{
    public class RecieveClass
    {
        /// <summary>
        /// key
        /// </summary>
        private int key;

        private const DEVICE_KEY = "MY_DEVICE_KEY";
        private const ACCESS_TOKEN = "MY_ACCESS_TOKEN";

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="key">key</param>
        public RecieveClass(int key)
        {
            this.key = key;
        }

        /// <summary>
        /// Get response data with dictionary using key for asynchronous.
        /// </summary>
        /// <returns>Dictionary</returns>
        private async Task<Dictionary<string, string>> GetResponseAsync()
        {
            const string COOKIE_VALUE = $"device_key={DEVICE_KEY}; access_token={ACCESS_TOKEN}; _gat=1";
            string URL_VALUE = "http://send-anywhere.com/web/key/search/" + key.ToString("D6");

            JObject json = null;

            using (var client = new HttpClient())
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_VALUE);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.Headers["Cookie"] = COOKIE_VALUE;

                WebResponse response = null;

                try
                {
                    response = await request.GetResponseAsync();
                }
                catch (WebException e) //exception (404 Not Found, 403 Forbidden, 401 Unauthorized)
                {
                        Dictionary<string, string> json_dict = new Dictionary<string, string>();
                        json_dict.Add("error", e.Message);

                        json = JObject.Parse(DictionaryToJson(json_dict));
                        return json.ToObject<Dictionary<string, string>>();
                }

                var respStream = response.GetResponseStream();
                respStream.Flush();

                using (StreamReader sr = new StreamReader(respStream))
                {
                    string strContent = sr.ReadToEnd();
                    respStream = null;

                    json = JObject.Parse(strContent);
                }
            }
            return json.ToObject<Dictionary<string, string>>();
        }

        /// <summary>
        /// Get download link (url) using key for asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetLinkAsync()
        {
            Dictionary<string, string> response = await GetResponseAsync();

            if (response.ContainsKey("error")) //response the error
                return string.Format("error: {0}", response["error"]);

            return response["weblink"];
        }

        /// <summary>
        /// Get download link with verbose (dictionary) using key for asynchronous.
        /// </summary>
        /// <returns>Dictionary (with verbose)</returns>
        public async Task<Dictionary<string, string>> GetLinkWithVerboseAsync()
        {
            return await GetResponseAsync();
        }

        /// <summary>
        /// Convert Dictionary Type to Json Type.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private string DictionaryToJson(Dictionary<string, string> dict)
        {
            return JsonConvert.SerializeObject(dict);
        }
    }
}
