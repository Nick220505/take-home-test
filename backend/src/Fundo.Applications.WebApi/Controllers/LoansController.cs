using Fundo.Applications.WebApi.Application.Loans;
using Fundo.Applications.WebApi.Contracts.Loans;
using Fundo.Applications.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("loans")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoanResponse>> Create([FromBody] CreateLoanRequest request, CancellationToken ct)
        {
            var loan = await _loanService.CreateAsync(request.Amount, request.ApplicantName, ct);
            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, LoanResponse.From(loan));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoanResponse>> GetById(Guid id, CancellationToken ct)
        {
            var loan = await _loanService.GetAsync(id, ct);

            if (loan == null)
            {
                throw new KeyNotFoundException("Loan not found.");
            }

            return Ok(LoanResponse.From(loan));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LoanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LoanResponse>>> List(CancellationToken ct)
        {
            var loans = await _loanService.ListAsync(ct);
            return Ok(loans.Select(LoanResponse.From));
        }

        [HttpPost("{id:guid}/payment")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoanResponse>> ApplyPayment(Guid id, [FromBody] ApplyPaymentRequest request, CancellationToken ct)
        {
            var loan = await _loanService.ApplyPaymentAsync(id, request.Amount, ct);
            return Ok(LoanResponse.From(loan));
        }

    }
}
