namespace Diplom.Models
{
    public class Genres
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Связь с книгами (одна категория → много книг)
        public virtual ICollection<Book>? Books { get; set; }
    }
}
