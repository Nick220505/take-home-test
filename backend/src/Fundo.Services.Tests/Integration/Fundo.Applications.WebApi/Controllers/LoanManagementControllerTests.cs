using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public LoanManagementControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetBalances_ShouldReturnExpectedResult()
        {
            var response = await _client.GetAsync("/loan");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
