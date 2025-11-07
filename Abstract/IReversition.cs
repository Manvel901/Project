using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Diplom.Abstract
{
    public interface IReservation
    {
        ReservationDto CreateReservation(int userId, int bookId, string bookTitle);

        Task<ReservationDto> ReserveBookByTitleAndAuthor(string title, string author, ClaimsPrincipal user);
        
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
