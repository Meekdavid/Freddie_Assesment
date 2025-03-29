using Freddie.Helpers.Services;
using System.ComponentModel.DataAnnotations;

namespace Freddie.Models
{
    public class Candidate
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Key]
        public string Email { get; set; }

        public string ResumeUrl { get; set; }
        public string ResumeText { get; set; }

        // Screening Questions
        public string KeyStrengths { get; set; }
        public string BiggestWeakness { get; set; }
        public bool AvailableImmediately { get; set; }
        
        public bool? Contacted { get; set; }
        public DateTime? ContactedDate { get; set; }

        // Metadata
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Evaluation Object
        public CandidateEvaluation AIEvaluation { get; set; }
    }
}
