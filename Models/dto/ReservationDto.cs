namespace Diplom.Models.dto
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public string? BookTitle { get; set; }     // Название книги (не BookId!)
        public List<string>? AuthorsName { get; set; } = new List<string>();
        public DateTime ReservationDate { get; set; }= DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public string? Status { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
       
    }
}
