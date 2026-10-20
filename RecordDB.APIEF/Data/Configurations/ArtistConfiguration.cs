using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Data.Configurations
{
    public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
    {
        public void Configure(EntityTypeBuilder<Artist> builder)
        {
            builder.ToTable("Artist");

            builder.HasKey(a => a.ArtistId);

            builder.Property(a => a.ArtistId)
                   .UseIdentityColumn();

            builder.Property(a => a.FirstName)
                   .HasMaxLength(50);

            builder.Property(a => a.LastName)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(a => a.Name)
                   .HasMaxLength(50);

            builder.Property(a => a.Biography)
                   .HasColumnType("text");

            builder.HasMany(a => a.Records)
                   .WithOne(r => r.Artist)
                   .HasForeignKey(r => r.ArtistId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
