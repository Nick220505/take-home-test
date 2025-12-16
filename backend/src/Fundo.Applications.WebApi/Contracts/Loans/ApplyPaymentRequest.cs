using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Contracts.Loans
{
    public class ApplyPaymentRequest
    {
        [Required]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Amount { get; set; }
    }
}
