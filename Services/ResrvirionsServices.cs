using Diplom.Abstract;
using Diplom.Models;
using Microsoft.EntityFrameworkCore;
using static Diplom.Models.ApDbContext;

namespace Diplom.Services
{
    public class ResrvirionsServices : IReversition

    {
        private readonly AppDbContext _context;
        private readonly IBookServicrs _bookService;
        private readonly IEmailService _emailService;
        public ResrvirionsServices(AppDbContext context, IBookServicrs bookService, IEmailService emailService)
        {
            _context = context;
            _bookService = bookService;
            _emailService = emailService;
        }
        public decimal CalculatePenalty(int reservationId)
        {
            var reservation = _context.Reserv.Find(reservationId);
            if (reservation == null || reservation.Status != "Overdue")
                return 0;

            // Пример: 10 руб./день просрочки
            var daysOverdue = (DateTime.UtcNow - reservation.DueDate).Days;
            return daysOverdue * 10;
        }

        public void CancelReservation(int reservationId)
        {
            var reservation = _context.Reserv
            .Include(r => r.Book)
            .FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null) return;



            // Возвращаем книгу в доступные
            reservation.Book.AvailableCopies++;
            _context.Books.Update(reservation.Book);

            _context.Reserv.Remove(reservation);
            _context.SaveChanges();
        }

        public Reservation CreateReservation(int userId, int bookId)
        {
            // Проверка существования пользователя и книги
            var user = _context.Users.Find(userId);
            var book = _context.Books.Find(bookId);
            if (user == null || book == null)
                throw new KeyNotFoundException("Пользователь или книга не найдены.");

            // Проверка доступности книги
            if (!_bookService.IsBookAvailable(bookId))
                throw new InvalidOperationException("Книга недоступна для бронирования.");

            var reservation = new Reservation
            {
                UserId = userId,
                BookId = bookId,
                ReservationDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14), // Срок возврата: 14 дней
                Status = "Active",
                ReturnDate = null
            };

          user = _context.Users.Find(userId);
          book = _context.Books.Find(bookId);

            _emailService.SendReservationConfirmationAsync(
                user.Email,
                book.BookTitle,
                reservation.DueDate
            ).ConfigureAwait(false); // Асинхронно без ожидания

            return reservation;
            // Уменьшаем количество доступных экземпляров
            book.AvailableCopies--;
            _context.Books.Update(book);

            _context.Reserv.Add(reservation);
            _context.SaveChanges();

            return reservation;
        }

        public List<Reservation> GetBookReservations(int bookId)
        {
            return _context.Reserv
            .Include(r => r.User)
            .Where(r => r.BookId == bookId)
            .ToList();
        }

        public List<Reservation> GetOverdueReservations()
        {
            return _context.Reserv
            .Include(r => r.Book)
            .Include(r => r.User)
            .Where(r => r.DueDate < DateTime.UtcNow && r.Status == "Active")
            .ToList();
        }

        public Reservation GetReservationById(int reservationId)
        {
            return _context.Reserv
            .Include(r => r.Book)
            .Include(r => r.User)
            .FirstOrDefault(r => r.Id == reservationId);
        }

        public List<Reservation> GetUserReservations(int userId)
        {
            return _context.Reserv
            .Include(r => r.Book)
            .Where(r => r.UserId == userId)
            .ToList();
        }

        public void RefreshReservationStatuses()
        {
            var activeReservations = _context.Reserv
           .Where(r => r.Status == "Active")
           .ToList();

            foreach (var reservation in activeReservations)
            {
                if (reservation.DueDate < DateTime.UtcNow)
                {
                    reservation.Status = "Overdue";
                    _context.Reserv.Update(reservation);
                }
            }

            _context.SaveChanges();
        }

        public void UpdateReservationStatus(int reservationId, string status)
        {
            var reservation = _context.Reserv.Find(reservationId);
            if (reservation == null) return;

            reservation.Status = status;
            _context.SaveChanges();
        }
    }
}
