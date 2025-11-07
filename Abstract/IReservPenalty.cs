using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IReservPenalty
    {
        decimal GetOutstandingBalanceByReservation(int reservationId);

        // Возврат книги (увеличивает AvailableCopies)
        void ReturnBook(int bookId);

        // Рассчитать штраф за просрочку
        decimal CalculatePenalty(int reservationId);

        public bool HasOutstandingPenaltiesByReservation(int reservationId);

        public IEnumerable<PenaltyDto> GetPenaltiesByUserReservation(int reservationId);

        
    }
}
