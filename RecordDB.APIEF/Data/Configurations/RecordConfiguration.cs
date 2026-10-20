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
                   .HasMaxLength(50);
        }
    }
}
