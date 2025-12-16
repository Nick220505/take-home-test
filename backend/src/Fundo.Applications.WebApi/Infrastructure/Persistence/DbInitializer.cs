using Fundo.Applications.WebApi.Domain.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fundo.Applications.WebApi.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider services, bool seedDataEnabled, bool allowReseed)
        {
            using var scope = services.CreateScope();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbInitializer");

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            const int maxAttempts = 10;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (db.Database.IsRelational())
                    {
                        db.Database.Migrate();
                    }
                    else
                    {
                        db.Database.EnsureCreated();
                    }

                    if (!seedDataEnabled)
                    {
                        return;
                    }

                    var seedApplicantNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "John Doe",
                        "Jane Smith",
                        "Robert Johnson"
                    };

                    if (allowReseed)
                    {
                        var existingSeedLoans = db.Loans
                            .Where(x => seedApplicantNames.Contains(x.ApplicantName))
                            .ToList();

                        if (existingSeedLoans.Count > 0)
                        {
                            db.Loans.RemoveRange(existingSeedLoans);
                            db.SaveChanges();
                        }
                    }

                    var now = DateTimeOffset.UtcNow;

                    var seedLoans = new[]
                    {
                        new Loan
                        {
                            Id = Guid.NewGuid(),
                            Amount = 25000.00m,
                            CurrentBalance = 18750.00m,
                            ApplicantName = "John Doe",
                            Status = LoanStatus.Active,
                            CreatedAt = now
                        },
                        new Loan
                        {
                            Id = Guid.NewGuid(),
                            Amount = 15000.00m,
                            CurrentBalance = 0m,
                            ApplicantName = "Jane Smith",
                            Status = LoanStatus.Paid,
                            CreatedAt = now.AddMinutes(-15)
                        },
                        new Loan
                        {
                            Id = Guid.NewGuid(),
                            Amount = 50000.00m,
                            CurrentBalance = 32500.00m,
                            ApplicantName = "Robert Johnson",
                            Status = LoanStatus.Active,
                            CreatedAt = now.AddMinutes(-30)
                        }
                    };

                    var anyAdded = false;

                    foreach (var seedLoan in seedLoans)
                    {
                        if (!db.Loans.Any(x => x.ApplicantName == seedLoan.ApplicantName))
                        {
                            db.Loans.Add(seedLoan);
                            anyAdded = true;
                        }
                    }

                    if (anyAdded)
                    {
                        db.SaveChanges();
                    }

                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    logger.LogWarning(ex, "Database initialization attempt {Attempt} of {MaxAttempts} failed.", attempt, maxAttempts);
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Min(10, attempt)));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database initialization failed.");
                    throw;
                }
            }
        }
    }
}
