namespace Diplom.Models.dto
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string BookTitle { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public string Type { get; set; } // "Penalty", "Overdue", "Info"
    }
}
