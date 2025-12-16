using Fundo.Applications.WebApi.Domain.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fundo.Applications.WebApi.Infrastructure.Persistence.Configurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> entity)
        {
            entity.ToTable("Loans");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.CurrentBalance).HasColumnType("decimal(18,2)");

            entity.Property(x => x.ApplicantName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
        }
    }
}
