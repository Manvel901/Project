namespace Diplom.Models
{
    public class Penalties
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now.AddDays(15);
       

        // Связь с бронированием
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
    }
}
