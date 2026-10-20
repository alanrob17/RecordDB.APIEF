namespace RecordDB.APIEF.DTOs
{
    /// <summary>
    /// Slim projection for records that have a review.
    /// </summary>
    public class RecordReviewDto
    {
        public string? Name   { get; set; }
        public string? Title  { get; set; }
        public string  Review { get; set; } = string.Empty;
    }
}
