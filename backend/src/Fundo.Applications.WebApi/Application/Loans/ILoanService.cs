using Fundo.Applications.WebApi.Domain.Loans;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Application.Loans
{
    public interface ILoanService
    {
        Task<Loan> CreateAsync(decimal amount, string applicantName, CancellationToken ct = default);
        Task<Loan?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Loan>> ListAsync(CancellationToken ct = default);
        Task<Loan> ApplyPaymentAsync(Guid id, decimal amount, CancellationToken ct = default);
    }
}
