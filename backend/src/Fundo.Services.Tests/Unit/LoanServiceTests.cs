using FluentAssertions;
using Fundo.Applications.WebApi.Application.Loans;
using Fundo.Applications.WebApi.Domain.Loans;
using Fundo.Applications.WebApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Unit
{
    public class LoanServiceTests
    {
        [Fact]
        public async Task CreateAsync_WhenAmountIsZero_ShouldThrow()
        {
            await using var db = CreateDbContext();
            var sut = new LoanService(db);

            var act = async () => await sut.CreateAsync(0m, "John Doe");

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task CreateAsync_WhenApplicantNameIsWhitespace_ShouldThrow()
        {
            await using var db = CreateDbContext();
            var sut = new LoanService(db);

            var act = async () => await sut.CreateAsync(100m, "   ");

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateActiveLoanWithFullBalance()
        {
            await using var db = CreateDbContext();
            var sut = new LoanService(db);

            var loan = await sut.CreateAsync(123.45m, "  Alice  ");

            loan.ApplicantName.Should().Be("Alice");
            loan.Amount.Should().Be(123.45m);
            loan.CurrentBalance.Should().Be(123.45m);
            loan.Status.Should().Be(LoanStatus.Active);

            var persisted = await db.Loans.SingleAsync(x => x.Id == loan.Id);
            persisted.ApplicantName.Should().Be("Alice");
        }

        [Fact]
        public async Task ApplyPaymentAsync_WhenLoanMissing_ShouldThrowNotFound()
        {
            await using var db = CreateDbContext();
            var sut = new LoanService(db);

            var act = async () => await sut.ApplyPaymentAsync(Guid.NewGuid(), 1m);

            await act.Should().ThrowAsync<System.Collections.Generic.KeyNotFoundException>();
        }

        [Fact]
        public async Task ApplyPaymentAsync_WhenPaymentIsZero_ShouldThrow()
        {
            await using var db = CreateDbContext();
            var loan = await SeedLoanAsync(db, amount: 100m, currentBalance: 100m, status: LoanStatus.Active);
            var sut = new LoanService(db);

            var act = async () => await sut.ApplyPaymentAsync(loan.Id, 0m);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task ApplyPaymentAsync_WhenOverpay_ShouldThrow()
        {
            await using var db = CreateDbContext();
            var loan = await SeedLoanAsync(db, amount: 100m, currentBalance: 50m, status: LoanStatus.Active);
            var sut = new LoanService(db);

            var act = async () => await sut.ApplyPaymentAsync(loan.Id, 50.01m);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ApplyPaymentAsync_WhenLoanAlreadyPaid_ShouldThrow()
        {
            await using var db = CreateDbContext();
            var loan = await SeedLoanAsync(db, amount: 100m, currentBalance: 0m, status: LoanStatus.Paid);
            var sut = new LoanService(db);

            var act = async () => await sut.ApplyPaymentAsync(loan.Id, 1m);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ApplyPaymentAsync_WhenPayingFullBalance_ShouldMarkPaid()
        {
            await using var db = CreateDbContext();
            var loan = await SeedLoanAsync(db, amount: 100m, currentBalance: 25m, status: LoanStatus.Active);
            var sut = new LoanService(db);

            var updated = await sut.ApplyPaymentAsync(loan.Id, 25m);

            updated.CurrentBalance.Should().Be(0m);
            updated.Status.Should().Be(LoanStatus.Paid);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"UnitTestDb-{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }

        private static async Task<Loan> SeedLoanAsync(AppDbContext db, decimal amount, decimal currentBalance, LoanStatus status)
        {
            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                CurrentBalance = currentBalance,
                ApplicantName = "Seeded",
                Status = status,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Loans.Add(loan);
            await db.SaveChangesAsync();

            return loan;
        }
    }
}
