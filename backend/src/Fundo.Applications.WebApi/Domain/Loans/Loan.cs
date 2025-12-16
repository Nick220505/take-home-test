using System;
using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Domain.Loans
{
    public class Loan
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }

        [MaxLength(200)]
        public string ApplicantName { get; set; } = string.Empty;

        public LoanStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
