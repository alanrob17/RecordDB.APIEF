namespace RecordDB.APIEF.Models
{
    /// <summary>
    /// Represents a Record entity (album). Minimal stub for Artist navigation in Phase 1.
    /// </summary>
    public class Record
    {
        public int     RecordId  { get; set; }
        public int     ArtistId  { get; set; }
        public string? Name      { get; set; }

        public Artist? Artist    { get; set; }
    }
}
