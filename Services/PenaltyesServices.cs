using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Ocsp;
using static Diplom.Models.AppDbContext;

namespace Diplom.Services
{
    public class PenaltyesServices : IPenaltyService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PenaltyesServices> _logger;

        public PenaltyesServices(AppDbContext context, IMapper mapper, IMemoryCache cache, ILogger<PenaltyesServices> logger)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        // Ручное создание штрафа администратором
        public int CreatePenalty(PenaltyDto penaltyDto, int reservationId)
        {
            if (penaltyDto.Amount <= 0) throw new ArgumentException("Amount must be positive.");
            if (string.IsNullOrWhiteSpace(penaltyDto.BookTitle)) throw new ArgumentException("BookTitle required.");

            var reservationExists = _context.Reserv.Any(r => r.Id == reservationId);
            if (!reservationExists) throw new KeyNotFoundException("Reservation not found.");

            var entity = _mapper.Map<Penalties>(penaltyDto);
            if (entity.IssueDate == default) entity.IssueDate = DateTime.UtcNow;

            _context.Penalties.Add(entity);
            _context.SaveChanges();

            // Добавляем связь в явную join-таблицу
            _context.RservPenals.Add(new RservPenal { ReservationId = reservationId, PenaltyId = entity.Id });
            _context.SaveChanges();

            // Создаем уведомление для пользователя
            CreatePenaltyNotification(entity.UserId, entity.BookTitle, entity.Amount);

            return entity.Id;
        }

        // Автоматическое создание штрафа за просрочку
        public int CreateOverduePenalty(int reservationId)
        {
            var reservation = _context.Reserv
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null) throw new KeyNotFoundException("Reservation not found.");
            if (reservation.ReturnDate.HasValue) throw new InvalidOperationException("Book already returned.");

            var daysOverdue = (DateTime.UtcNow - reservation.DueDate).Days;
            if (daysOverdue <= 0) throw new InvalidOperationException("Book is not overdue.");

            // Расчет штрафа: 2% от стоимости книги за каждый день просрочки
            var bookPrice = reservation.Book?.Price ?? 100m; // Если цена не указана, берем 100 руб
            var penaltyAmount = bookPrice * 0.02m * daysOverdue;

            var penalty = new Penalties
            {
                UserId = reservation.UserId,
                Amount = Math.Round(penaltyAmount, 2),
                BookTitle = reservation.BookTitle,
                IssueDate = DateTime.UtcNow,
                AmountPaid = 0,
                IsCancelled = false
            };

            _context.Penalties.Add(penalty);
            _context.SaveChanges();

            // Связываем штраф с бронированием
            _context.RservPenals.Add(new RservPenal { ReservationId = reservationId, PenaltyId = penalty.Id });

            // Обновляем статус бронирования на "Просрочено"
            reservation.Status = "Overdue";
            _context.SaveChanges();

            // Создаем уведомление
            CreateOverdueNotification(reservation.UserId, reservation.BookTitle, penalty.Amount, daysOverdue);

            _logger.LogInformation("Created overdue penalty for reservation {ReservationId}. Amount: {Amount}, Days: {Days}",
                reservationId, penalty.Amount, daysOverdue);

            return penalty.Id;
        }

        public int CheckAndCreateOverduePenalties()
        {
            var overdueReservations = _context.Reserv
                .Include(r => r.Book)
                .Where(r => !r.ReturnDate.HasValue &&
                           r.DueDate < DateTime.UtcNow &&
                           r.Status != "Overdue")
                .ToList();

            var createdCount = 0;

            foreach (var reservation in overdueReservations)
            {
                try
                {
                    CreateOverduePenalty(reservation.Id);
                    createdCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating overdue penalty for reservation {ReservationId}", reservation.Id);
                }
            }

            return createdCount;
        }

        // Создание уведомления о штрафе
        private void CreatePenaltyNotification(int userId, string bookTitle, decimal amount)
        {
            var notification = new Notification
            {
                UserId = userId,
                BookTitle = "Новый штраф",
                Message = $"Вам назначен штраф за книгу '{bookTitle}' в размере {amount} руб.",
                Type = "Penalty",
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notification.Add(notification);
            _context.SaveChanges();
        }

        // Создание уведомления о просрочке
        private void CreateOverdueNotification(int userId, string bookTitle, decimal amount, int daysOverdue)
        {
            var notification = new Notification
            {
                UserId = userId,
                BookTitle = "Просрочка возврата книги",
                Message = $"Книга '{bookTitle}' просрочена на {daysOverdue} дней. Начислен штраф: {amount} руб.",
                Type = "Overdue",
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notification.Add(notification);
            _context.SaveChanges();
        }

        public IEnumerable<PenaltyDto> GetUserPenalties(int userId)
        {
            // Находим все бронирования пользователя
            var userReservations = _context.Reserv
                .Where(r => r.UserId == userId)
                .Select(r => r.Id)
                .ToList();

            // Находим все связи бронирований и штрафов
            var penaltyIds = _context.RservPenals
                .Where(rp => userReservations.Contains(rp.ReservationId))
                .Select(rp => rp.PenaltyId)
                .ToList();

            // Получаем штрафы
            var penalties = _context.Penalties
                .Where(p => penaltyIds.Contains(p.Id))
                .ToList();

            return _mapper.Map<IEnumerable<PenaltyDto>>(penalties);
        }

        public IEnumerable<PenaltyDto> GetPenaltiesByReservation(int reservationId)
        {
            var penaltyIds = _context.RservPenals
                .Where(rp => rp.ReservationId == reservationId)
                .Select(rp => rp.PenaltyId)
                .ToList();

            var penalties = _context.Penalties
                .Where(p => penaltyIds.Contains(p.Id))
                .ToList();

            return _mapper.Map<IEnumerable<PenaltyDto>>(penalties);
        }
        public bool PayPenalty(int id, decimal amountPaid, DateTime? paidAtUtc)
        {
            using (_context)
            {
                if (amountPaid <= 0) throw new ArgumentException("Payment must be positive.");

                // ИСПРАВЛЕНИЕ: убрал условие x.IsCancelled из поиска
                var penal = _context.Penalties.FirstOrDefault(x => x.Id == id);
                if (penal == null || penal.IsCancelled) return false;

                var remaining = penal.Amount - penal.AmountPaid;
                var toApply = Math.Min(remaining, amountPaid);
                if (toApply <= 0) return false;

                penal.AmountPaid += toApply;
                if (penal.AmountPaid >= penal.Amount)
                {
                    penal.PaidAtUtc = (paidAtUtc?.ToUniversalTime() ?? DateTime.UtcNow);

                    // Создаем уведомление об оплате
                    CreatePaymentNotification(penal.UserId, penal.BookTitle, penal.AmountPaid);
                }

                _context.SaveChanges();
                return true;
            }
        }

        // Метод для создания уведомления об оплате
        private void CreatePaymentNotification(int userId, string bookTitle, decimal amountPaid)
        {
            var notification = new Notification
            {
                UserId = userId,
                BookTitle = "Оплата штрафа",
                Message = $"Вы успешно оплатили штраф за книгу '{bookTitle}' на сумму {amountPaid} руб.",
                Type = "Payment",
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notification.Add(notification);
            _context.SaveChanges();
        }

        public PenaltyDto? GetPenaltyById(int penaltyId)
        {
            using (_context)
            {
                var penalEntity = _context.Penalties.FirstOrDefault(x => x.Id == penaltyId);

                return _mapper.Map<PenaltyDto>(penalEntity);
            }
        }

      

        public IEnumerable<PenaltyDto> GetPenaltiesByBookTitle(string bookTitle)
        {
            using (_context)
            {
                var list = _context.Penalties
                   .Where(x => string.Equals(x.BookTitle, bookTitle, StringComparison.OrdinalIgnoreCase))
                   .OrderByDescending(x => x.IssueDate)
                   .ToList();

                return list.Select(x => _mapper.Map<PenaltyDto>(x));
            }
        }




        //public int ApplyOverduePenalties(DateTime asOfUtc)
        //{
        //    // Требует интеграции с сущностью reservation/loan. В заглушке — 0.
        //    return 0;
        //}

        //public bool CancelPenalty(PenaltyDto penaltyDto)
        //{
        //    using (_context)
        //    {
        //        var penal = _context.Penalties.FirstOrDefault(x => x.Id == penaltyDto.Id);
        //        if (penal == null) return false;
        //        if (penal.IsCancelled) return false;
        //        penal.IsCancelled = true;
        //        if (!string.IsNullOrWhiteSpace(penaltyDto.))
        //            penal.BookTitle = $"{penal.BookTitle} (Cancelled: {reason})";
        //        _context.SaveChanges();
        //        return true;
        //    }
        //}


    }
}

