using Diplom.Models;
using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IPenaltyService
    {
        int CreatePenalty(PenaltyDto penaltyDto,int reservationId);
        bool PayPenalty(int id, int amountPaid, DateTime? paidAtUtc);
        PenaltyDto? GetPenaltyById(int penaltyId);
        
        IEnumerable<PenaltyDto> GetPenaltiesByBookTitle(string bookTitle);
        
       // int ApplyOverduePenalties(DateTime asOfUtc);
       // bool CancelPenalty(PenaltyDto penaltyDto);
    
    }
}
