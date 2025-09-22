namespace Diplom.Models
{
    public class Autors
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string LastName { get; set; }

        public string Bio { get; set; }

        // Связь многие-ко-многим с книгами
        public virtual ICollection<Book>? Books { get; set; }
    }
}
