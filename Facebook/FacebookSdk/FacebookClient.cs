using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Net;

namespace JH.Applications
{
    public interface IFacebookClient
    {
        Task<string> GetAsync(Action<string> callback, string accessToken, string endpoint, string args = null);
        Task PostAsync(string accessToken, string endpoint, object data, string args = null);
    }

    public class FacebookClient : IFacebookClient
    {
        private readonly HttpClient _httpClient;
        int count;

        public FacebookClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/v2.12/")
            };
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetAsync(Action<string> callback, string accessToken, string endpoint, string args = null)
        {
        repeat:
            var response = await _httpClient.GetAsync(String.Format("{0}?access_token={1}&{2}", endpoint, accessToken, args));
            
            var result = await response.Content.ReadAsStringAsync();
            
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    if (result.Contains("User request limit reached") || result.Contains("Calls to this api have exceeded the rate limit"))
                    {
                        callback("Waiting 15 min\r\n");
                        callback(DateTime.Now.ToString()+"\r\n");
                        for (int i = 0; i < 15; i++)
                        {
                            Thread.Sleep(60000);
                            callback(".");
                        }
                        callback("\r\n");
                        //callback(response.ToString());
                        //callback("\r\n");
                        goto repeat;
                    }
                    else
                    {
                        return null;
                    }
                case HttpStatusCode.Forbidden:
                    return null;
            }
         
            count++;
            return result;
        }

        public async Task PostAsync(string accessToken, string endpoint, object data, string args = null)
        {
            var payload = GetPayload(data);
            String s = String.Format("{0}?access_token={1}&{2}", endpoint, accessToken,args);
            await _httpClient.PostAsync(s, payload);
        }

        private static StringContent GetPayload(object data)
        {
            var json = JsonConvert.SerializeObject(data);

            return new StringContent(json, Encoding.UTF8, "application/json");
        }

    }
}