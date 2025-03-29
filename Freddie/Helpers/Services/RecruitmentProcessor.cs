using Common.ConfigurationSettings;
using Freddie.Helpers.Database;
using Freddie.Models;
using Microsoft.EntityFrameworkCore;

namespace Freddie.Helpers.Services
{
    public class RecruitmentProcessor
    {
        private readonly GoogleSheetsService _sheetsService;
        private readonly ResumeProcessor _resumeProcessor;
        private readonly ApplicationDbContext _dbContext;
        private readonly OpenAiEvaluationService _evaluationService;
        private readonly SmtpEmailService _emailService;
        private readonly ILogger<RecruitmentProcessor> _logger;

        public RecruitmentProcessor(
            GoogleSheetsService sheetsService,
            ResumeProcessor resumeProcessor,
            ApplicationDbContext dbContext,
            ILogger<RecruitmentProcessor> logger,
            OpenAiEvaluationService evaluationService,
            SmtpEmailService emailService)
        {
            _sheetsService = sheetsService;
            _resumeProcessor = resumeProcessor;
            _dbContext = dbContext;
            _logger = logger;
            _evaluationService = evaluationService;
            _emailService = emailService;
        }

        /// <summary>
        /// Processes candidates by fetching them from Google Sheets, processing their resumes,
        /// evaluating them using OpenAI, and storing/updating their information in the database.
        /// </summary>
        /// <returns>APIResponse containing the result of the processing.</returns>
        public async Task<APIResponse> ProcessCandidatesAsync()
        {
            var response = new APIResponse();
            response.SpreadSheetUrl = string.Empty;
            response.Candidates = new List<CandidateResponse>();

            response.ResponseCode = "01";
            response.ResponseMessage = "Unable to perform ranking";
            try
            {
                // 1. Fetch candidates from Google Sheets
                var candidates = await _sheetsService.GetCandidatesAsync();
                _logger.LogInformation("Retrieved {Count} candidates from Google Sheets", candidates.Count);

                foreach (var candidate in candidates)
                {
                    // 2. Process resume if not already processed
                    if (string.IsNullOrEmpty(candidate.ResumeText) && !string.IsNullOrEmpty(candidate.ResumeUrl))
                    {
                        candidate.ResumeText = await _resumeProcessor.ExtractResumeTextAsync(candidate.ResumeUrl);
                        _logger.LogInformation("Processed resume for {Name}", candidate.FullName);
                    }

                    // 3. Add/update candidate information in the database
                    var existingCandidate = await _dbContext.Candidates
                        .FirstOrDefaultAsync(c => c.Email == candidate.Email);

                    if (existingCandidate == null)
                    {
                        // Add new candidate to the database
                        _dbContext.Candidates.Add(candidate);
                    }
                    else
                    {
                        // Update existing candidate information
                        existingCandidate.FullName = candidate.FullName;
                        existingCandidate.ResumeUrl = candidate.ResumeUrl;
                        existingCandidate.ResumeText = candidate.ResumeText;
                        existingCandidate.KeyStrengths = candidate.KeyStrengths;
                        existingCandidate.BiggestWeakness = candidate.BiggestWeakness;
                        existingCandidate.AvailableImmediately = candidate.AvailableImmediately;
                        existingCandidate.ModifiedDate = DateTime.UtcNow;
                    }

                    // 4. Evaluate candidate if not already evaluated
                    if (candidate?.AIEvaluation?.Score == null || candidate?.AIEvaluation?.Score == 0)
                    {
                        var evaluation = await _evaluationService.EvaluateCandidateAsync(candidate, ConfigSettings.ApplicationSetting.JobRoleToEvaluateCandidate);
                        candidate.AIEvaluation = new CandidateEvaluation();

                        candidate.AIEvaluation.Score = evaluation.Score;
                        candidate.AIEvaluation.EvaluationDate = evaluation.EvaluationDate;
                        candidate.AIEvaluation.EvaluationNotes = evaluation.EvaluationNotes;

                        // Send email notification for qualified candidates
                        if (evaluation.Score >= ConfigSettings.ApplicationSetting.CandidateEligibilityThreshold)
                        {
                            await _emailService.SendQualificationEmail(candidate);
                            candidate.Contacted = true;
                            candidate.ContactedDate = DateTime.Now;
                        }

                        // Store evaluation in the database
                        _dbContext.Evaluations.Add(evaluation);
                    }
                }

                if (candidates.Count > 0)
                {
                    // Store processed candidates back to Google Sheets
                    var spreadSheetUrl = await _sheetsService.StoreCandidatesAsync(candidates);

                    response.SpreadSheetUrl = spreadSheetUrl;
                    response.Candidates = candidates.Select(x => new CandidateResponse
                    {
                        Email = x.Email,
                        FullName = x.FullName,
                        AIEvaluation = x.AIEvaluation != null
                            ? new AIEvaluationResponse
                            {
                                Rating = x.AIEvaluation.Score,
                                EvaluationDate = x.AIEvaluation.EvaluationDate,
                                EvaluationNotes = x.AIEvaluation.EvaluationNotes
                            }
                        : null
                    }).ToList();


                    response.ResponseCode = "00";
                    response.ResponseMessage = "Ranking Successful";
                }

                // 5. Save all changes to the database
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully processed all candidates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing candidates");
            }

            return response;
        }
    }
}
