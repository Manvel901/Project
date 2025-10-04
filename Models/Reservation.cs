using Diplom.Models.dto;

namespace Diplom.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string? BookTitle { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public DateTime? ReturnDate { get; set; } = null;
        public string?  Status { get; set; }
        public string? Comment { get; set; }
        public bool IsBlocked { get; set; }

        // Внешние ключи
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }
       
      
        public virtual ICollection<RservPenal>? ResPen { get; set; }
       
    }
}
