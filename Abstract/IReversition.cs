using Diplom.Models;

namespace Diplom.Abstract
{
    public interface IReversition
    {
        Reservation CreateReservation(int userId, int bookId);

        // Отменить бронирование
        void CancelReservation(int reservationId);

        // Получить бронирование по ID
        Reservation GetReservationById(int reservationId);

        // Получить все бронирования пользователя
        List<Reservation> GetUserReservations(int userId);

        // Получить просроченные бронирования
        List<Reservation> GetOverdueReservations();

        // Обновить статус бронирования (например, при возврате книги)
        void UpdateReservationStatus(int reservationId, string status);

        // Рассчитать штраф за просрочку
        decimal CalculatePenalty(int reservationId);

        // Получить все бронирования для книги
        List<Reservation> GetBookReservations(int bookId);

        // Автоматически обновить статусы бронирований (просрочены/активны)
        void RefreshReservationStatuses();
    }
}
