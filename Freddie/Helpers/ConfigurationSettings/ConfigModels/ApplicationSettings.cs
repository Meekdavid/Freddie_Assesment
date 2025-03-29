using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ConfigurationSettings.ConfigModels
{
    public class ApplicationSettings
    {
        public EmailDetails EmailDetails { get; set; }
        public FireBaseStorage FireBaseStorage { get; set; }
        public JwtConfig JwtConfig { get; set; }
        public string SpreadsheetId { get; set; }
        public string OpenAIKey { get; set; }
        public string JobRoleToEvaluateCandidate { get; set; }
        public string ServiceAccountKeyPath { get; set; }
        public string FreddieGmail { get; set; }
        public double CandidateEligibilityThreshold { get; set; }
        
        public int RetryCountForDatabaseTransactions { get; set; }
        public int RetryCountForExceptions { get; set; }
        public string MaximumTokenEdebAI { get; set; }
        public string OpenAITemperature { get; set; }
        public string Key { get; set; }
        public int SecondsBetweenEachRetry { get; set; }
        public int CacheDuration { get; set; }
    }
}
