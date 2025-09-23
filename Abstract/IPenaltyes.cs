using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IPenaltyService
    {
        int CreatePenalty(PenaltyDto penaltyDto);
        bool PayPenalty(PenaltyDto penaltyDto);
        PenaltyDto? GetPenaltyById(int penaltyId);
        IEnumerable<PenaltyDto> GetPenaltiesByUserReservation(int reservationId);
        IEnumerable<PenaltyDto> GetPenaltiesByBookTitle(string bookTitle);
        decimal GetOutstandingBalanceByReservation(int reservationId);
        int ApplyOverduePenalties(DateTime asOfUtc);
       // bool CancelPenalty(PenaltyDto penaltyDto);
        bool HasOutstandingPenaltiesByReservation(int reservationId);
    }
}
