using System.Text.Json.Serialization;

namespace RecordDB.APIEF.Models
{
    /// <summary>
    /// Represents an Artist entity mapped to the database via Entity Framework Core.
    /// </summary>
    public class Artist
    {
        public int     ArtistId  { get; set; }
        public string? FirstName { get; set; }
        public string  LastName  { get; set; } = string.Empty;
        public string? Name      { get; set; }
        public string? Biography { get; set; }

        /// <summary>
        /// Navigation property to Records by this artist.
        /// </summary>
        [JsonIgnore]
        public ICollection<Record> Records { get; set; } = new List<Record>();

        public override string ToString()
        {
            var biography = string.IsNullOrEmpty(Biography)
                ? "No Biography"
                : (Biography.Length > 30 ? Biography[..30] + "..." : Biography);

            return $"Artist Id: {ArtistId}, Artist: {Name}, Biography: {biography}";
        }
    }
}
