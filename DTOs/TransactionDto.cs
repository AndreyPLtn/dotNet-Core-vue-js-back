namespace AccountingApp.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public required string Currency { get; set; }
        public decimal Amount { get; set; }
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
    }
}
