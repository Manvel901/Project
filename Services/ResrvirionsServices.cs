using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using static Diplom.Models.AppDbContext;

namespace Diplom.Services
{
    public class ResrvirionsServices : IReservation

    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IBookServicrs _bookService;
        private readonly ILogger<ResrvirionsServices> _logger;
     

        public ResrvirionsServices(AppDbContext context, IBookServicrs bookService,
             IMapper mapper, IMemoryCache cache, ILogger<ResrvirionsServices> logger)
        {

            _context = context;
            _bookService = bookService;
            _logger = logger;
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

        public ReservationDto CreateReservation(int userId, int bookId, string bookTitle)
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
                BookTitle = bookTitle,
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

        public async Task<ReservationDto> ReserveBookByTitleAndAuthor(string title, string author, ClaimsPrincipal user)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                throw new InvalidOperationException("Title and author are required.");

            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Нормализация для поиска
            var normalizedTitle = title.Trim();
            var normalizedAuthor = author.Trim();

            // Логирование
            _logger.LogInformation("Ищем книгу с титульником '{Title}' и автором '{Author}'", normalizedTitle, normalizedAuthor);

            // Поиск книги по названию и автору
            var book = await _context.Books
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.BookTitle == normalizedTitle && b.Authors.Any(a => a.FullName == normalizedAuthor));

            if (book == null)
                throw new KeyNotFoundException($"Книга '{title}' автор '{author}' не найдена.");

            // Проверка на существующее бронирование
            var existingReservation = await _context.Reserv
                .FirstOrDefaultAsync(r => r.BookId == book.Id && r.UserId == userId);

            if (existingReservation != null)
                throw new InvalidOperationException("Это бронирование уже существует.");

            // Создаем DTO для бронирования
            var reservationDto = new ReservationDto
            {
                BookId = book.Id,
                BookTitle = book.BookTitle,
                AuthorsName = book.Authors.Select(a => a.FullName).ToList(),
                ReservationDate = DateTime.UtcNow
            };

            // Создание резервирования в контексте
            var reservatEntity = _mapper.Map<Reservation>(reservationDto); // Преобразование DTO в сущность Reservation
            reservatEntity.UserId = userId; // Устанавливаем идентификатор пользователя
            _context.Reserv.Add(reservatEntity); // Добавление бронирования в контекст

            await _context.SaveChangesAsync(); // Асинхронное сохранение изменений в базе данных

            return reservationDto; // Возвращаем DTO бронирования
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
           
            
                var list = _context.Reserv
               .Include(r => r.Book)
               .Where(r => r.UserId == userId);

                return list.Select(x=> _mapper.Map<ReservationDto>(x));


            
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
