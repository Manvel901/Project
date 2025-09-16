using Diplom.Abstract;
using Diplom.Models;
using Microsoft.EntityFrameworkCore;
using static Diplom.Models.ApDbContext;

namespace Diplom.Services
{
    public class AnaliticService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnaliticService(AppDbContext context)
        {
            _context = context;
        }
        public List<BookStatistic> GetTopBooks(int topCount, DateTime? startDate, DateTime? endDate)
        {
       

        
            var query = _context.Reserv
                .Include(r => r.Book)
                .AsQueryable();

            if (startDate != null)
                query = query.Where(r => r.ReservationDate >= startDate);

            if (endDate != null)
                query = query.Where(r => r.ReservationDate <= endDate);

            return query
                .GroupBy(r => r.BookId)
                .Select(g => new BookStatistic
                {
                    BookId = g.Key,
                    Title = g.First().Book.BookTitle,
                    ReservationCount = g.Count()
                })
                .OrderByDescending(b => b.ReservationCount)
                .Take(topCount)
                .ToList();
        }
    }
    
}
