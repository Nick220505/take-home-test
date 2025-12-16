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
    }
}
