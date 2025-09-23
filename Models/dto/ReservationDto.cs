namespace Diplom.Models.dto
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public string BookTitle { get; set; }     // Название книги (не BookId!)
        public DateTime ReservationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public int PenaltyId { get; set; }
    }
}
