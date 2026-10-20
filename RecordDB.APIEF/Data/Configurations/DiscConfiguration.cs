using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Data.Configurations
{
    public class DiscConfiguration : IEntityTypeConfiguration<Disc>
    {
        public void Configure(EntityTypeBuilder<Disc> builder)
        {
            builder.ToTable("Disc");

            builder.HasKey(d => d.DiscId);

            builder.Property(d => d.DiscId)
                   .UseIdentityColumn();

            builder.Property(d => d.RecordId)
                   .IsRequired();

            builder.Property(d => d.DiscNo)
                   .IsRequired();

            builder.Property(d => d.FreeDbId)
                   .HasMaxLength(10);

            builder.HasMany(d => d.Tracks)
                   .WithOne(t => t.Disc)
                   .HasForeignKey(t => t.DiscId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
