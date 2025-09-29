using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Diplom.Services
{
    public class ReservPenalty : IReservPenalty
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public ReservPenalty(AppDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public IEnumerable<PenaltyDto> GetPenaltiesByUserReservation(int reservationId)
        {
            return _context.RservPenals
                .Where(rp => rp.ReservationId == reservationId)
                .Select(rp => rp.Penalty)
                .OrderByDescending(p => p.IssueDate)
                .Select(p => _mapper.Map<PenaltyDto>(p))
                .ToList();
        }
        public decimal GetOutstandingBalanceByReservation(int reservationId)
        {
            return _context.RservPenals
                .Where(rp => rp.ReservationId == reservationId
                             && !rp.Penalty.IsCancelled)
                .Sum(rp => Math.Max(0m, rp.Penalty.Amount - rp.Penalty.AmountPaid));
        }
        public bool HasOutstandingPenaltiesByReservation(int reservationId)
        {
            return _context.RservPenals
                .Any(rp => rp.ReservationId == reservationId
                           && !rp.Penalty.IsCancelled
                           && rp.Penalty.Amount > rp.Penalty.AmountPaid);
        }
        public decimal CalculatePenalty(int reservationId)
        {
            using (_context)
            {
                var reservation = _context.Reserv.Find(reservationId);
                if (reservation == null || reservation.Status != "Overdue")
                    return 0;

                // Пример: 10 руб./день просрочки
                var daysOverdue = (DateTime.UtcNow - reservation.DueDate).Days;
                return daysOverdue * 10;
            }
        }
       
        public void ReturnBook(int bookId)
        {
            using (_context)
            {
                var book = _context.Books.Find(bookId);
                if (book == null) throw new KeyNotFoundException("Книга не найдена.");

                book.AvailableCopies++;
                _context.SaveChanges();
            }
        }
    }
}
