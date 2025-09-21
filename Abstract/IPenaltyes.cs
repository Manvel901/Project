using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IPenaltyService
    {
        int CreatePenalty(PenaltyDto penaltyDto);
        bool PayPenalty(int penaltyId, decimal amountPaid, DateTime? paidAt = null);
        PenaltyDto? GetPenaltyById(int penaltyId);
        IEnumerable<PenaltyDto> GetPenaltiesByUserReservation(int reservationId);
        IEnumerable<PenaltyDto> GetPenaltiesByBookTitle(string bookTitle);
        decimal GetOutstandingBalanceByReservation(int reservationId);
        int ApplyOverduePenalties(DateTime asOfUtc);
        bool CancelPenalty(int penaltyId, string? reason = null);
        bool HasOutstandingPenaltiesByReservation(int reservationId);
    }
}
