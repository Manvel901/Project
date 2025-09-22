namespace Diplom.Models
{
    public class Penalties
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string BookTitle { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now.AddDays(15);
        public decimal AmountPaid { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? PaidAtUtc { get; set; }

        // Связь с бронированием
        public int ReservationId { get; set; }
        public virtual Reservation? Reservation { get; set; }
    }
}
