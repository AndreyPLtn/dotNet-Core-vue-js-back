using System.ComponentModel.DataAnnotations;

namespace AccountingApp.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }

        [Required]
        public required string Currency { get; set; }
        public decimal Balance { get; set; }
    }
}
