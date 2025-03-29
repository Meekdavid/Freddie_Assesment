using Common.ConfigurationSettings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Sheets.v4;

namespace Freddie.Helpers.Services
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _config;
        private GoogleCredential _credential;

        public GoogleAuthService(IConfiguration config)
        {
            _config = config;
            Authenticate();
        }

        private void Authenticate()
        {
            string credentialPath = ConfigSettings.ApplicationSetting.ServiceAccountKeyPath;

            if (string.IsNullOrEmpty(credentialPath))
            {
                throw new InvalidOperationException("Google API credential path is not configured.");
            }

            _credential = GoogleCredential.FromFile(credentialPath)
                .CreateScoped(new[]
                {
                SheetsService.Scope.Spreadsheets,
                DriveService.Scope.Drive,
                GmailService.Scope.GmailSend,
                GmailService.Scope.GmailSettingsBasic,
                GmailService.Scope.GmailMetadata,
                GmailService.Scope.GmailModify,
                GmailService.Scope.GmailSettingsSharing,
                GmailService.Scope.GmailCompose,
                GmailService.Scope.MailGoogleCom,
                GmailService.Scope.GmailReadonly,

                });
        }

        public GoogleCredential GetCredential() => _credential;
    }

}
