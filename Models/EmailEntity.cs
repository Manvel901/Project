namespace Diplom.Models
{
    public class EmailEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int Rating { get; set; }
        public virtual ICollection<User>? Users { get; set; }

        


    }
}
