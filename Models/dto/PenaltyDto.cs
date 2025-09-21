namespace Diplom.Models.dto
{
    public class PenaltyDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
      
        public string BookTitle { get; set; }     // Для связи с книгой
        public decimal AmountPaid { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? PaidAtUtc { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now.AddDays(15);


        // Связь с бронированием
        public int ReservationId { get; set; }
    }
}
