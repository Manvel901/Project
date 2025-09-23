using Diplom.Models.dto;

namespace Diplom.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);
        public DateTime? ReturnDate { get; set; }   = null;
        public string  Status { get; set; }
        public string Comment { get; set; }
        public bool IsBlocked { get; set; }

        // Внешние ключи
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }
       
        public int PenaltyId { get; set; }
        public virtual Penalties? Penalty { get; set; }
       
    }
}
