using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Contracts.Loans
{
    public class CreateLoanRequest
    {
        [Required]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(200)]
        public string ApplicantName { get; set; } = string.Empty;
    }
}
