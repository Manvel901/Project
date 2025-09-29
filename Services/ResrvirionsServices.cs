using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static Diplom.Models.AppDbContext;

namespace Diplom.Services
{
    public class ResrvirionsServices : IReservation

    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IBookServicrs _bookService;
     

        public ResrvirionsServices(AppDbContext context, IBookServicrs bookService,
             IMapper mapper, IMemoryCache cache )
        {

            _context = context;
            _bookService = bookService;
            
            _mapper = mapper;
            _cache = cache;
        }


      

        public void CancelReservation(int reservationId)
        {
            using (_context)
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
        }

        public ReservationDto CreateReservation(int userId, int bookId)
        {
            // не диспоузим _context, он из DI
            var user = _context.Users.Find(userId);
            var book = _context.Books.Find(bookId);
            if (user == null || book == null)
                throw new KeyNotFoundException("Пользователь или книга не найдены.");

            if (book.AvailableCopies == null || book.AvailableCopies <= 0)
                throw new InvalidOperationException("Книга недоступна для бронирования.");

            var reservation = new Reservation
            {
                UserId = userId,
                BookId = bookId,
                ReservationDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                Status = "Active"
            };

            // обновляем книгу
            book.AvailableCopies--;
            _context.Books.Update(book);

            // сохраняем сначала entity, чтобы получить Id
            _context.Reserv.Add(reservation);
            _context.SaveChanges();

            // мапим DTO после SaveChanges и дополняем поля, которые не мапятся автоматически
            var resDto = _mapper.Map<ReservationDto>(reservation);
            resDto.BookTitle = book.BookTitle; // если DTO содержит BookTitle
                                           // при необходимости заполните другие поля: resDto.UserName = user.Name;

            return resDto;
        }
        

        public IEnumerable<ReservationDto> GetBookReservations(int bookId)
        {
            using (_context)
            {
                var list = _context.Reserv
                .Include(r => r.User)
                .Where(r => r.BookId == bookId);
                
                return list.Select(x=> _mapper.Map<ReservationDto>(x)).ToList();
            }
        }

        public IEnumerable<ReservationDto> GetOverdueReservations()
        {
            using (_context)
            {
                var list = _context.Reserv
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.DueDate < DateTime.UtcNow && r.Status == "Active");

                return list.Select(x => _mapper.Map<ReservationDto>(x)).ToList();
            }
            
        }

        public ReservationDto GetReservationById(int reservationId)
        {
            using (_context)
            {
                return _mapper.Map<ReservationDto>(_context.Reserv
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == reservationId));
            }

              
        }

        public IEnumerable<ReservationDto> GetUserReservations(int userId)
        {
            using (_context)
            {
                var list = _context.Reserv
               .Include(r => r.Book)
               .Where(r => r.UserId == userId);

                return list.Select(x=> _mapper.Map<ReservationDto>(x));


            }
        }

        public void RefreshReservationStatuses()
        {
            using (_context)
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
        }

        public void UpdateReservationStatus( ReservationDto reservationDto)
        {
            var reservation = _context.Reserv.Find(reservationDto.Id);
            if (reservation == null) return;

            reservation.Status = reservationDto.Status;
            _context.SaveChanges();
        }
    }
}
