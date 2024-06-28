namespace AccountingApp.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string? Currency { get; set; }
        public decimal Balance { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}