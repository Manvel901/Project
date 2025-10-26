using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Abstract
{
    public interface IReservation
    {
        ReservationDto CreateReservation(int userId, int bookId, string bookTitle);

         Task<ReservationDto> CreateReservationByTitle(string title, string author);

        // Отменить бронирование
        void CancelReservation(int reservationId);

        // Получить бронирование по ID
        ReservationDto GetReservationById(int reservationId);

        // Получить все бронирования пользователя
        IEnumerable<ReservationDto> GetUserReservations(int userId);

        // Получить просроченные бронирования
        IEnumerable<ReservationDto> GetOverdueReservations();

        // Обновить статус бронирования (например, при возврате книги)
        void UpdateReservationStatus( ReservationDto reservationDto);

        

        // Получить все бронирования для книги
        IEnumerable<ReservationDto> GetBookReservations(int bookId);

        // Автоматически обновить статусы бронирований (просрочены/активны)
        void RefreshReservationStatuses();
    }
}
