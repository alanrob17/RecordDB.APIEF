using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Data.Configurations
{
    public class TrackConfiguration : IEntityTypeConfiguration<Track>
    {
        public void Configure(EntityTypeBuilder<Track> builder)
        {
            builder.ToTable("Track");

            builder.HasKey(t => t.TrackId);

            builder.Property(t => t.TrackId)
                   .UseIdentityColumn();

            builder.Property(t => t.DiscId)
                   .IsRequired();

            builder.Property(t => t.Name)
                   .HasMaxLength(255);

            builder.Property(t => t.Extended)
                   .HasMaxLength(255);
        }
    }
}
