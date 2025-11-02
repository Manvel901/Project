namespace Diplom.Models
{
    public class EmailEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public virtual ICollection<User>? Users { get; set; }

        


    }
}
