namespace Diplom.Models.dto
{
    public class PenaltyDto
    {
        public decimal Amount { get; set; }
      
        public string BookTitle { get; set; }     // Для связи с книгой
        public DateTime DueDate { get; set; }
    }
}
