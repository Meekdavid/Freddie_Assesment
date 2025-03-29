using Common.ConfigurationSettings;
using Newtonsoft.Json;
using System.Text;

namespace Freddie.Helpers.Services
{
    public class OpenAIServiceCustom
    {
        private readonly ApiClient _apiClient;

        public OpenAIServiceCustom(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<string> CallOpenAiApiAsync(string prompt)
        {
            var url = "https://api.openai.com/v1/chat/completions";

            var payload = new
            {
                model = "gpt-4o-mini",
                store = true,
                messages = new[]
                {
                        new { role = "user", content = prompt }
                    }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {ConfigSettings.ApplicationSetting.OpenAIKey}" }
                };

            return await _apiClient.PostAsync(url, content, headers);
        }
    }
}
