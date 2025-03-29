using Common.ConfigurationSettings;
using Freddie.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Freddie.Helpers.Services
{
    public class GoogleSheetsService
    {
        private SheetsService _sheetsService;
        private GoogleAuthService _authService;
        private readonly ResumeProcessor _driveService;
        private readonly IConfiguration _config;
        private readonly ILogger<GoogleSheetsService> _logger;

        public GoogleSheetsService(IConfiguration config, ILogger<GoogleSheetsService> logger, ResumeProcessor driveService, GoogleAuthService authService)
        {
            _config = config;
            _logger = logger;
            _driveService = driveService;
            _authService = authService;

            try
            {
                // Generate access token manually
                string accessToken = GenerateAccessTokenAsync().GetAwaiter().GetResult();

                _sheetsService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = authService.GetCredential(),
                    ApplicationName = "Freddie AI Recruiter"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize GoogleSheetsService");
                throw;
            }
        }

        private async Task<string> GenerateAccessTokenAsync()
        {
            try
            {
                // Load service account key
                var json = System.IO.File.ReadAllText("skillful-hope-403905-625543ab556a.json");
                var serviceAccount = JsonConvert.DeserializeObject<ServiceAccountKey>(json);

                serviceAccount.PrivateKey = serviceAccount.PrivateKey.Replace("-----BEGIN PRIVATE KEY-----", "")
                    .Replace("-----END PRIVATE KEY-----", "")
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Trim();

                var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var expiresAt = issuedAt + 300; // Expire in 5 minutes

                var claims = new List<Claim>
                    {
                        new Claim("iss", serviceAccount.ClientEmail),
                        new Claim("scope", "https://www.googleapis.com/auth/spreadsheets.readonly"),
                        new Claim("aud", "https://oauth2.googleapis.com/token"),
                        new Claim("iat", issuedAt.ToString()),
                        new Claim("exp", expiresAt.ToString())
                    };

                var rsa = new RSACryptoServiceProvider();
                rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(serviceAccount.PrivateKey), out _);
                var securityKey = new RsaSecurityKey(rsa);

                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
                var token = new JwtSecurityToken(
                    claims: claims,
                    signingCredentials: credentials,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(1));

                string jwt = new JwtSecurityTokenHandler().WriteToken(token);

                // Exchange JWT for OAuth2 access token
                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                        new KeyValuePair<string, string>("assertion", jwt)
                    });

                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(jsonResponse);

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate access token");
                throw;
            }
        }

        public async Task<List<Candidate>> GetCandidatesAsync()
        {
            try
            {
                string spreadsheetId = ConfigSettings.ApplicationSetting.SpreadsheetId;
                string range = "Sheet1!A2:F";

                var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = await request.ExecuteAsync();

                return response.Values.Select(row => new Candidate
                {
                    FullName = row[0].ToString(),
                    Email = row[1].ToString(),
                    ResumeUrl = row[2].ToString(),
                    KeyStrengths = row[3].ToString(),
                    BiggestWeakness = row[4].ToString(),
                    AvailableImmediately = row[5].ToString().Equals("yes", StringComparison.OrdinalIgnoreCase)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get candidates from Google Sheets");
                throw;
            }
        }

        public async Task<string> StoreCandidatesAsync(List<Candidate> candidates, string spreadsheetName = "Candidate Evaluations")
        {
            try
            {
                // 1. Create new spreadsheet
                var spreadsheet = new Spreadsheet
                {
                    Properties = new SpreadsheetProperties
                    {
                        Title = spreadsheetName,
                        Locale = "en_US"
                    }
                };

                var createRequest = _sheetsService.Spreadsheets.Create(spreadsheet);
                var createdSpreadsheet = await createRequest.ExecuteAsync();
                var spreadsheetId = createdSpreadsheet.SpreadsheetId;

                // 2. Prepare data for writing
                var valueRange = new ValueRange
                {
                    Values = new List<IList<object>>
                        {
                            // Header row
                            new List<object>
                            {
                                "Full Name", "Email", "AI Rating", "Evaluation Date",
                                "Key Strengths", "Biggest Weakness", "Available Immediately",
                                "Resume URL", "Contacted", "Contacted Date"
                            }
                        }
                };

                // Add candidate data
                foreach (var candidate in candidates)
                {
                    valueRange.Values.Add(new List<object>
                        {
                            candidate.FullName,
                            candidate.Email,
                            candidate.AIEvaluation.Score,
                            candidate.AIEvaluation.EvaluationDate,
                            candidate.KeyStrengths,
                            candidate.BiggestWeakness,
                            candidate.AvailableImmediately ? "Yes" : "No",
                            candidate.ResumeUrl,
                            candidate.Contacted.ToString(),
                            candidate.ContactedDate?.ToString("yyyy-MM-dd") ?? ""
                        });
                }

                // 3. Write data to sheet
                var updateRequest = _sheetsService.Spreadsheets.Values.Update(
                    valueRange,
                    spreadsheetId,
                    "A1:J" + (candidates.Count + 1));

                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                await updateRequest.ExecuteAsync();

                // 4. Format header row
                var formatRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Request>
                        {
                            new Request
                            {
                                RepeatCell = new RepeatCellRequest
                                {
                                    Range = new GridRange
                                    {
                                        SheetId = 0,
                                        StartRowIndex = 0,
                                        EndRowIndex = 1
                                    },
                                    Cell = new CellData
                                    {
                                        UserEnteredFormat = new CellFormat
                                        {
                                            TextFormat = new TextFormat { Bold = true },
                                            BackgroundColor = new Color { Red = 0.9f, Green = 0.9f, Blue = 0.9f }
                                        }
                                    },
                                    Fields = "userEnteredFormat(textFormat,backgroundColor)"
                                }
                            }
                        }
                };

                await _sheetsService.Spreadsheets.BatchUpdate(formatRequest, spreadsheetId).ExecuteAsync();

                // 5. Set sharing permissions (make accessible)
                var permission = new Permission
                {
                    Type = "anyone",
                    Role = "writer" // Set to "reader" if you only want viewing access
                };

                await _driveService.GrantPermissionAsync(permission, spreadsheetId);

                return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store candidates in Google Sheets");
                throw;
            }
        }
    }

    // Helper Class to Parse JSON Key
    public class ServiceAccountKey
    {
        [JsonProperty("client_email")]
        public string ClientEmail { get; set; }

        [JsonProperty("private_key")]
        public string PrivateKey { get; set; }
    }

    // Helper Class for OAuth Token Response
    public class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    // Custom Credential Class for Google API
    public class CustomTokenCredential : IConfigurableHttpClientInitializer
    {
        private readonly string _accessToken;

        public CustomTokenCredential(string accessToken)
        {
            _accessToken = accessToken;
        }

        public void Initialize(ConfigurableHttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        }
    }
}
