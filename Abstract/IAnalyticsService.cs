using Diplom.Models;

namespace Diplom.Abstract
{
    public interface IAnalyticsService
    {
        // Топ-N популярных книг за период
        List<BookStatistic> GetTopBooks(int topCount, DateTime? startDate, DateTime? endDate);
    }
}
