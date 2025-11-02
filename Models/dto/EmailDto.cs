namespace Diplom.Models.dto
{
    public class EmailDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
    }
}
