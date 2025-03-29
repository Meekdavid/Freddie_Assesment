using Common.ConfigurationSettings;
using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace Freddie.Helpers.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAsync(string url, Dictionary<string, string> queryParams = null, Dictionary<string, string> headers = null)
        {
            var uriBuilder = new UriBuilder(url);
            if (queryParams != null)
            {
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                foreach (var param in queryParams)
                {
                    query[param.Key] = param.Value;
                }
                uriBuilder.Query = query.ToString();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string url, HttpContent content, Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }

    public class HttpRequestHeaders
    {
        public string Accept { get; set; }
        public string Authorization { get; set; }
    }

    public class HttpRequestContent
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    public class HttpRequestDetails
    {
        public HttpMethod Method { get; set; }
        public Uri RequestUri { get; set; }
        public HttpRequestHeaders Headers { get; set; }
        public HttpRequestContent Content { get; set; }
    }

    public class AIAPIResponse
    {
        public Openai openai { get; set; }
    }

    public class Openai
    {
        public string generated_text { get; set; }
        public Messaget[] message { get; set; }
        public string status { get; set; }
        public Usage usage { get; set; }
        public float cost { get; set; }
    }

    public class Usage
    {
        public int completion_tokens { get; set; }
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
        public Completion_Tokens_Details completion_tokens_details { get; set; }
        public Prompt_Tokens_Details prompt_tokens_details { get; set; }
    }

    public class Completion_Tokens_Details
    {
        public int accepted_prediction_tokens { get; set; }
        public int audio_tokens { get; set; }
        public int reasoning_tokens { get; set; }
        public int rejected_prediction_tokens { get; set; }
    }

    public class Prompt_Tokens_Details
    {
        public int audio_tokens { get; set; }
        public int cached_tokens { get; set; }
    }

    public class Messaget
    {
        public string role { get; set; }
        public string message { get; set; }
        public object tools { get; set; }
        public object[] tool_calls { get; set; }
    }



    public class ClientAIRequest
    {
        public string providers { get; set; }
        public string text { get; set; }
        public string chatbot_global_action { get; set; }
        public Previous_History[] previous_history { get; set; }
        public int temperature { get; set; }
        public int max_tokens { get; set; }
    }

    public class Previous_History
    {
        public string role { get; set; }
        public string message { get; set; }
    }
}
