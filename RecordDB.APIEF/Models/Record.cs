using System.Text.Json.Serialization;

namespace RecordDB.APIEF.Models
{
    /// <summary>
    /// Represents a Record (album) as stored in the database.
    /// </summary>
    public class Record
    {
        public int       RecordId  { get; set; }
        public int       ArtistId  { get; set; }
        public string    Name      { get; set; } = string.Empty;
        public string    Field     { get; set; } = string.Empty;
        public int       Recorded  { get; set; }
        public string    Label     { get; set; } = string.Empty;
        public string    Pressing  { get; set; } = string.Empty;
        public string    Rating    { get; set; } = string.Empty;
        public int       Discs     { get; set; }
        public string    Media     { get; set; } = string.Empty;
        public DateTime? Bought    { get; set; }
        public decimal?  Cost      { get; set; }
        public string?   CoverName { get; set; }
        public string?   Review    { get; set; }
        public int?      FreeDbId  { get; set; }

        // Navigation properties
        [JsonIgnore]
        public Artist?              Artist    { get; set; }

        [JsonIgnore]
        public ICollection<Disc>    DiscsList { get; set; } = new List<Disc>();

        public override string ToString() =>
            $"Record Id: {RecordId}, Name: {Name}, Recorded: {Recorded}, Media: {Media}";
    }
}
