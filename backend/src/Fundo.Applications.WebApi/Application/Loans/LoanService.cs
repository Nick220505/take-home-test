using Fundo.Applications.WebApi.Domain.Loans;
using Fundo.Applications.WebApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Application.Loans
{
    public class LoanService : ILoanService
    {
        private readonly AppDbContext _db;

        public LoanService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Loan> CreateAsync(decimal amount, string applicantName, CancellationToken ct = default)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(applicantName))
            {
                throw new ArgumentException("Applicant name is required.", nameof(applicantName));
            }

            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                CurrentBalance = amount,
                ApplicantName = applicantName.Trim(),
                Status = LoanStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Loans.Add(loan);
            await _db.SaveChangesAsync(ct);

            return loan;
        }

        public Task<Loan?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Loans
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<Loan>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Loans
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Loan> ApplyPaymentAsync(Guid id, decimal amount, CancellationToken ct = default)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
            }

            var loan = await _db.Loans.SingleOrDefaultAsync(x => x.Id == id, ct);

            if (loan == null)
            {
                throw new KeyNotFoundException("Loan not found.");
            }

            if (loan.Status == LoanStatus.Paid)
            {
                throw new InvalidOperationException("Loan is already paid.");
            }

            if (amount > loan.CurrentBalance)
            {
                throw new InvalidOperationException("Payment amount exceeds current balance.");
            }

            loan.CurrentBalance -= amount;

            if (loan.CurrentBalance == 0m)
            {
                loan.Status = LoanStatus.Paid;
            }

            await _db.SaveChangesAsync(ct);

            return loan;
        }
    }
}
