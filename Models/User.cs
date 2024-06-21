namespace AccountingApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public List<Account>? Accounts { get; set; }
    }
}   