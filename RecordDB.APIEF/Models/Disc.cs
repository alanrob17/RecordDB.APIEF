namespace RecordDB.APIEF.Models
{
    /// <summary>
    /// Represents a Disc within a Record.
    /// </summary>
    public class Disc
    {
        public int     DiscId       { get; set; }
        public int     RecordId     { get; set; }
        public int     DiscNo       { get; set; }
        public int?    FreeDbDiscId { get; set; }
        public string? FreeDbId     { get; set; }
        public int?    Length       { get; set; }

        // Navigation properties
        public Record?             Record { get; set; }
        public ICollection<Track>  Tracks { get; set; } = new List<Track>();
    }
}
