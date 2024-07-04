using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class CurrencyRate
    {   
        public int Id { get; set; }

        [Required]
        public required string FromCurrency { get; set; }

        [Required]
        public required string ToCurrency { get; set; }

        [Required]
        public decimal Rate { get; set; }
    }
}