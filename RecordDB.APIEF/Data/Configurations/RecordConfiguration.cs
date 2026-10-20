using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Data.Configurations
{
    public class RecordConfiguration : IEntityTypeConfiguration<Record>
    {
        public void Configure(EntityTypeBuilder<Record> builder)
        {
            builder.ToTable("Record");

            builder.HasKey(r => r.RecordId);

            builder.Property(r => r.RecordId)
                   .UseIdentityColumn();

            builder.Property(r => r.ArtistId)
                   .IsRequired();

            builder.Property(r => r.Name)
                   .HasMaxLength(80)
                   .IsRequired();

            builder.Property(r => r.Field)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.Recorded)
                   .IsRequired();

            builder.Property(r => r.Label)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.Pressing)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.Rating)
                   .HasMaxLength(4)
                   .IsRequired();

            builder.Property(r => r.Discs)
                   .IsRequired();

            builder.Property(r => r.Media)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.Bought)
                   .HasColumnType("datetime");

            builder.Property(r => r.Cost)
                   .HasColumnType("money");

            builder.Property(r => r.CoverName)
                   .HasMaxLength(50);

            builder.Property(r => r.Review)
                   .HasColumnType("text");

            builder.Property(r => r.FreeDbId)
                   .HasColumnName("FreeDBID");

            builder.HasOne(r => r.Artist)
                   .WithMany(a => a.Records)
                   .HasForeignKey(r => r.ArtistId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.DiscsList)
                   .WithOne(d => d.Record)
                   .HasForeignKey(d => d.RecordId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
