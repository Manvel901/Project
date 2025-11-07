namespace Diplom.Models
{
    public class Penalties
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string? BookTitle { get; set; }
        public DateTime IssueDate { get; set; } 
        public decimal AmountPaid { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? PaidAtUtc { get; set; }

      

        public virtual ICollection<RservPenal>? ResPen { get; set; }
        public virtual User? User { get; set; }
    }
}
