using Common.ConfigurationSettings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text.RegularExpressions;

namespace Freddie.Helpers.Services
{
    public class ResumeProcessor
    {
        private readonly DriveService _driveService;
        private readonly IConfiguration _config;
        private GoogleAuthService _authService;
        private readonly ILogger<ResumeProcessor> _logger;

        public ResumeProcessor(IConfiguration config, ILogger<ResumeProcessor> logger, GoogleAuthService authService)
        {
            _config = config;
            _logger = logger;
            _authService = authService;

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = authService.GetCredential(),
                ApplicationName = "Freddie AI Recruiter"
            });
            
        }

        public async Task<string> ExtractResumeTextAsync(string resumeUrl)
        {
            try
            {
                // 1. Extract file ID
                var fileId = ExtractFileIdFromUrl(resumeUrl);
                if (string.IsNullOrEmpty(fileId))
                {
                    _logger.LogWarning("Invalid Google Drive URL: {Url}", resumeUrl);
                    return string.Empty;
                }

                // 2. Download as PDF bytes
                var request = _driveService.Files.Get(fileId);
                request.SupportsAllDrives = true;
                using var stream = new MemoryStream();
                await request.DownloadAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                // 3. Extract text from PDF
                return ExtractTextFromPdfStream(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from resume: {Url}", resumeUrl);
                return string.Empty;
            }
        }

        private string ExtractTextFromPdfStream(MemoryStream pdfStream)
        {
            try
            {
                // Option A: Using iTextSharp (LGPL license)
                using (var reader = new PdfReader(pdfStream))
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var text = PdfTextExtractor.GetTextFromPage(reader, 1, strategy);
                    return text;
                }

                /* 
                // Option B: Using PdfPig (MIT license)
                using (var document = PdfDocument.Open(pdfStream))
                {
                    return string.Join(" ", document.GetPages()
                        .SelectMany(p => p.GetWords())
                        .Select(w => w.Text));
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF text extraction failed");
                return string.Empty;
            }
        }

        public async Task GrantPermissionAsync(Permission permission, string spreadSheetId)
        {
            await _driveService.Permissions.Create(permission, spreadSheetId).ExecuteAsync();
        }

        private string ExtractFileIdFromUrl(string url)
        {
            var uri = new Uri(url);
            // Handle different Google Drive URL formats
            if (uri.Host.Contains("drive.google.com"))
            {
                var match = Regex.Match(uri.LocalPath, @"/file/d/([^/]+)");
                if (match.Success) return match.Groups[1].Value;

                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["id"] ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
