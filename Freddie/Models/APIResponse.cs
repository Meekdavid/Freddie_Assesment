namespace Freddie.Models
{
    public class APIResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string SpreadSheetUrl { get; set; }
        public List<CandidateResponse> Candidates { get; set; }
    }
}
