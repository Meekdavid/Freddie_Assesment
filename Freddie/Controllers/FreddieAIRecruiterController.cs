using Freddie.Helpers.Services;
using Freddie.Models;
using Microsoft.AspNetCore.Mvc;

namespace Freddie.Controllers
{
    [ApiController]
    [Route("api/")]
    public class FreddieAIRecruiterController : Controller
    {
        private readonly RecruitmentProcessor _candidateService;
        private readonly ILogger<FreddieAIRecruiterController> _logger;
        public FreddieAIRecruiterController(RecruitmentProcessor candidateService, ILogger<FreddieAIRecruiterController> logger)
        {
            _candidateService = candidateService;
            _logger = logger;
        }
        /// <summary>
        /// Fetches candidate evaluation results with ranking information.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/rankings`  
        ///  
        /// This endpoint retrieves all candidate evaluations sorted by score (descending) with their complete assessment details.
        ///
        /// **Response Includes:**  
        /// - Candidate personal information (Name, Email)  
        /// - Evaluation score (0-100)  
        /// - Screening answers  
        /// - Resume analysis summary  
        /// - Contact status  
        ///
        /// **Example Request (cURL):**  
        /// ```sh
        /// curl -X GET "/api/rankings" \
        /// -H "Accept: application/json"
        /// ```  
        ///
        /// **Response Codes:**  
        /// - **200** → Returns a list of candidate evaluations  
        /// - **500** → Server error while processing the request  
        /// </remarks>
        /// <returns>A ranked list of candidate evaluations.</returns>
        [HttpGet("rankings")]
        [ProducesResponseType(typeof(APIResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCandidateRankings()
        {
            try
            {
                var evaluations = await _candidateService.ProcessCandidatesAsync();
                return Ok(evaluations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candidate rankings");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving rankings");
            }
        }
    }
}
