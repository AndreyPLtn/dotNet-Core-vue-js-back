using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [Required]
        public required string Currency { get; set; }

        [Required]
        public required decimal Amount { get; set; }

        [Required]
        public required int FromAccountId { get; set; }

        [Required]
        public required int ToAccountId { get; set; }
    }
}