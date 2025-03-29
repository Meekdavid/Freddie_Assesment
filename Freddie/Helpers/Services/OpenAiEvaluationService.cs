using Common.ConfigurationSettings;
using Freddie.Models;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Freddie.Helpers.Services
{
    public class OpenAiEvaluationService
    {
        private readonly OpenAIService _openAiService;
        private readonly ILogger<OpenAiEvaluationService> _logger;

        public OpenAiEvaluationService(ILogger<OpenAiEvaluationService> logger)
        {
            _logger = logger;
            _openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = ConfigSettings.ApplicationSetting.EmailDetails.APIKey
            });
        }

        public async Task<CandidateEvaluation> EvaluateCandidateAsync(Candidate candidate, string jobRole)
        {
            var prompt = BuildEvaluationPrompt(candidate, jobRole);

            try
            {
                _logger.LogInformation("Evaluating candidate {Email} for job role {JobRole}", candidate.Email, jobRole);
                var response = await CallOpenAiApiAsync(prompt);
                var evaluation = ParseEvaluationResponse(response);

                _logger.LogInformation("Successfully evaluated candidate {Email}", candidate.Email);
                return new CandidateEvaluation
                {
                    CandidateId = candidate.Email,
                    Score = evaluation.Rate,
                    EvaluationNotes = evaluation.Details,
                    EvaluationDate = DateTime.Now.ToString("dd MMMM yyyy")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to evaluate candidate {Email}", candidate.Email);
                throw;
            }
        }

        private string BuildEvaluationPrompt(Candidate candidate, string jobRole)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Analyze this candidate for a {jobRole} position and provide a rating.");
            sb.AppendLine();
            sb.AppendLine("Candidate Information:");
            sb.AppendLine($"- Name: {candidate.FullName}");
            sb.AppendLine($"- Key Strengths: {candidate.KeyStrengths}");
            sb.AppendLine($"- Biggest Weakness: {candidate.BiggestWeakness}");
            sb.AppendLine($"- Available Immediately: {candidate.AvailableImmediately}");
            sb.AppendLine();
            sb.AppendLine("Resume Content:");
            sb.AppendLine(candidate.ResumeText);
            sb.AppendLine();
            sb.AppendLine("Evaluation Criteria:");
            sb.AppendLine("1. Relevant Experience");
            sb.AppendLine("2. Skills Match");
            sb.AppendLine("3. Cultural Fit");
            sb.AppendLine();
            sb.AppendLine("Required Output Format (JSON ONLY):");
            sb.AppendLine("{");
            sb.AppendLine("  \"rate\": 0-100,");
            sb.AppendLine("  \"details\": \"Concise analysis covering experience, skills, and cultural fit\"");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("Important Instructions:");
            sb.AppendLine("- Return ONLY valid JSON");
            sb.AppendLine("- Do not include any explanatory text outside the JSON");
            sb.AppendLine("- \"rate\" must be between 0-100");
            sb.AppendLine("- \"details\" should be 2-3 sentences");

            return sb.ToString();
        }

        public async Task<string> CallOpenAiApiAsync(string prompt)
        {
            _logger.LogInformation("Calling OpenAI API with prompt: {Prompt}", prompt);

            var previousHistory = new List<(string role, string message)>
                        {
                            ("user", ""),
                            ("assistant", "")
                        };

            string provider = "openai";
            string text = prompt;
            string chatbotAction = "You are an expert recruiter analyzing candidate fit.";
            string temperature = ConfigSettings.ApplicationSetting.OpenAITemperature;
            string maxTokens = ConfigSettings.ApplicationSetting.MaximumTokenEdebAI;

            var requestPayload = new
            {
                providers = provider,
                text = text,
                chatbot_global_action = chatbotAction,
                previous_history = previousHistory?.ConvertAll(h => new { role = h.role, message = h.message }),
                temperature = temperature,
                max_tokens = maxTokens
            };

            var jsonPayload = JsonConvert.SerializeObject(requestPayload);

            var requestDetails = new HttpRequestDetails
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.edenai.run/v2/text/chat"),
                Headers = new HttpRequestHeaders
                {
                    Accept = "application/json",
                    Authorization = $"Bearer {ConfigSettings.ApplicationSetting.Key}"
                },
                Content = new HttpRequestContent
                {
                    Content = jsonPayload,
                    ContentType = "application/json"
                }
            };

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = requestDetails.Method,
                RequestUri = requestDetails.RequestUri,
                Content = new StringContent(requestDetails.Content.Content)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue(requestDetails.Content.ContentType) }
                }
            };

            request.Headers.Add("Accept", requestDetails.Headers.Accept);
            request.Headers.Add("Authorization", requestDetails.Headers.Authorization);

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var deserializedResponse = JsonConvert.DeserializeObject<AIAPIResponse>(body);

                _logger.LogInformation("Received response from OpenAI API: {Response}", body);

                return deserializedResponse.openai.generated_text;
            }
        }

        private (int Rate, string Details) ParseEvaluationResponse(string jsonResponse)
        {
            try
            {
                var pattern = @"\{.*\}";
                var match = Regex.Match(jsonResponse, pattern, RegexOptions.Singleline);

                _logger.LogInformation("Parsing evaluation response");
                jsonResponse = match.Value;

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                return (
                    root.GetProperty("rate").GetInt32(),
                    root.GetProperty("details").GetString() ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI response: {Response}", jsonResponse);
                throw new FormatException("Invalid evaluation response format", ex);
            }
        }
    }

    public class CandidateEvaluation
    {
        public string Id { get; init; } = new Random().Next(1000, 9999).ToString(); // Random ID between 1000-9999
        public string CandidateId { get; set; }
        public int Score { get; set; }
        public string EvaluationNotes { get; set; }
        public string EvaluationDate { get; set; }
    }

}
