using Fundo.Applications.WebApi.Domain.Loans;
using System;

namespace Fundo.Applications.WebApi.Contracts.Loans
{
    public class LoanResponse
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public LoanStatus Status { get; set; }

        public static LoanResponse From(Loan loan)
        {
            ArgumentNullException.ThrowIfNull(loan);

            return new()
            {
                Id = loan.Id,
                Amount = loan.Amount,
                CurrentBalance = loan.CurrentBalance,
                ApplicantName = loan.ApplicantName,
                Status = loan.Status
            };
        }
    }
}
