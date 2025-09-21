using Diplom.Models;
using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IReversition
    {
        ReservationDto CreateReservation(int userId, int bookId);

        // Отменить бронирование
        void CancelReservation(int reservationId);

        // Получить бронирование по ID
        ReservationDto GetReservationById(int reservationId);

        // Получить все бронирования пользователя
        IEnumerable<ReservationDto> GetUserReservations(int userId);

        // Получить просроченные бронирования
        IEnumerable<ReservationDto> GetOverdueReservations();

        // Обновить статус бронирования (например, при возврате книги)
        void UpdateReservationStatus(int reservationId, string status);

        // Рассчитать штраф за просрочку
        decimal CalculatePenalty(int reservationId);

        // Получить все бронирования для книги
        IEnumerable<ReservationDto> GetBookReservations(int bookId);

        // Автоматически обновить статусы бронирований (просрочены/активны)
        void RefreshReservationStatuses();
    }
}
