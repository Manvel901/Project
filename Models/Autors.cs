namespace Diplom.Models
{
    public class Autors
    {
        public int Id { get; set; }
        public string FullName { get; set; }
      

        public string Bio { get; set; }

        // Связь многие-ко-многим с книгами
        public virtual ICollection<Book>? Books { get; set; }
    }
}
