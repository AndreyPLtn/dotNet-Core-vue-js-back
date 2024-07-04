using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public required string Currency { get; set; }
        public decimal Balance { get; set; }
        public required int UserId { get; set; }
        public required User User { get; set; }
    }
}