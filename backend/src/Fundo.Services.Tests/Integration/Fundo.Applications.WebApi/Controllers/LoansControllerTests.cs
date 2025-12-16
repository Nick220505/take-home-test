using FluentAssertions;
using Fundo.Applications.WebApi.Contracts.Loans;
using Fundo.Applications.WebApi.Domain.Loans;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class LoansControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public LoansControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _client.DefaultRequestHeaders.Add("X-Api-Key", "test-api-key");
        }

        [Fact]
        public async Task GetLoans_ShouldReturnSeededLoans()
        {
            var response = await _client.GetAsync("/loans");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var loans = await ReadAsync<List<LoanResponse>>(response);

            loans.Should().NotBeNull();
            loans!.Select(x => x.ApplicantName).Should().Contain(new[] { "John Doe", "Jane Smith", "Robert Johnson" });
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnCreatedLoan()
        {
            var request = new
            {
                amount = 1234.56m,
                applicantName = "Test Applicant"
            };

            var response = await _client.PostAsJsonAsync("/loans", request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var loan = await ReadAsync<LoanResponse>(response);

            loan.Should().NotBeNull();
            loan!.ApplicantName.Should().Be("Test Applicant");
            loan.Amount.Should().Be(1234.56m);
            loan.CurrentBalance.Should().Be(1234.56m);
            loan.Status.Should().Be(LoanStatus.Active);
        }

        [Fact]
        public async Task GetLoanById_WhenExists_ShouldReturnLoan()
        {
            var listResponse = await _client.GetAsync("/loans");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loans = await ReadAsync<List<LoanResponse>>(listResponse);
            var john = loans!.Single(x => x.ApplicantName == "John Doe");

            var response = await _client.GetAsync($"/loans/{john.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var loan = await ReadAsync<LoanResponse>(response);
            loan!.ApplicantName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetLoanById_WhenMissing_ShouldReturnNotFound()
        {
            var response = await _client.GetAsync($"/loans/{System.Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ApplyPayment_ShouldDeductFromCurrentBalance()
        {
            var listResponse = await _client.GetAsync("/loans");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loans = await ReadAsync<List<LoanResponse>>(listResponse);
            var john = loans!.Single(x => x.ApplicantName == "John Doe");

            var paymentRequest = new { amount = 1000m };

            var response = await _client.PostAsJsonAsync($"/loans/{john.Id}/payment", paymentRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await ReadAsync<LoanResponse>(response);
            updated!.CurrentBalance.Should().Be(john.CurrentBalance - 1000m);
        }

        [Fact]
        public async Task ApplyPayment_WhenOverpay_ShouldReturnBadRequest()
        {
            var listResponse = await _client.GetAsync("/loans");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loans = await ReadAsync<List<LoanResponse>>(listResponse);
            var john = loans!.Single(x => x.ApplicantName == "John Doe");

            var paymentRequest = new { amount = john.CurrentBalance + 1m };

            var response = await _client.PostAsJsonAsync($"/loans/{john.Id}/payment", paymentRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task ApplyPayment_WhenLoanAlreadyPaid_ShouldReturnBadRequest()
        {
            var listResponse = await _client.GetAsync("/loans");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loans = await ReadAsync<List<LoanResponse>>(listResponse);
            var jane = loans!.Single(x => x.ApplicantName == "Jane Smith");

            var paymentRequest = new { amount = 1m };

            var response = await _client.PostAsJsonAsync($"/loans/{jane.Id}/payment", paymentRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("Loan is already paid");
        }

        [Fact]
        public async Task ApplyPayment_WhenPayingFullBalance_ShouldMarkLoanAsPaid()
        {
            var listResponse = await _client.GetAsync("/loans");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loans = await ReadAsync<List<LoanResponse>>(listResponse);
            var robert = loans!.Single(x => x.ApplicantName == "Robert Johnson");

            var paymentRequest = new { amount = robert.CurrentBalance };

            var response = await _client.PostAsJsonAsync($"/loans/{robert.Id}/payment", paymentRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await ReadAsync<LoanResponse>(response);
            updated!.CurrentBalance.Should().Be(0m);
            updated.Status.Should().Be(LoanStatus.Paid);
        }

        [Fact]
        public async Task ApplyPayment_WhenAmountIsZero_ShouldReturnBadRequestWithValidationErrors()
        {
            var listResponse = await _client.GetAsync("/loans");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loans = await ReadAsync<List<LoanResponse>>(listResponse);
            var john = loans!.Single(x => x.ApplicantName == "John Doe");

            var paymentRequest = new { amount = 0m };

            var response = await _client.PostAsJsonAsync($"/loans/{john.Id}/payment", paymentRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

            using var doc = await ReadJsonDocumentAsync(response);
            doc.RootElement.TryGetProperty("errors", out _).Should().BeTrue();
        }

        [Fact]
        public async Task CreateLoan_WhenAmountIsZero_ShouldReturnBadRequestWithValidationErrors()
        {
            var request = new
            {
                amount = 0m,
                applicantName = "Test Applicant"
            };

            var response = await _client.PostAsJsonAsync("/loans", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

            using var doc = await ReadJsonDocumentAsync(response);
            doc.RootElement.TryGetProperty("errors", out _).Should().BeTrue();
        }

        [Fact]
        public async Task CreateLoan_WhenApplicantNameMissing_ShouldReturnBadRequestWithValidationErrors()
        {
            var request = new
            {
                amount = 100m
            };

            var response = await _client.PostAsJsonAsync("/loans", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

            using var doc = await ReadJsonDocumentAsync(response);
            doc.RootElement.TryGetProperty("errors", out _).Should().BeTrue();
        }

        private static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            });
        }

        private static async Task<JsonDocument> ReadJsonDocumentAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
    }
}
