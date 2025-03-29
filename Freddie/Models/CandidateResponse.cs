namespace Freddie.Models
{
    public class CandidateResponse
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public AIEvaluationResponse AIEvaluation { get; set; }
    }

    public class AIEvaluationResponse
    {
        public double? Rating { get; set; }
        public string? EvaluationDate { get; set; }
        public string EvaluationNotes { get; set; }
    }
}
