namespace Diplom.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string BookTitle { get; set; }
        public string ISBN { get; set; }
        public decimal Price { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string Status { get; set; }

        // Внешний ключ для жанра
        public int GenreId { get; set; }
        public virtual Genres? Genre { get; set; }

        public virtual ICollection<Autors>? Authors { get; set; }


        public virtual ICollection<Reservation>? Reservations { get; set; }

        
    }
}
