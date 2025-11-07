using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;

namespace Diplom.Services
{
    public class NotificationServices:INotificationServices
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationServices> _logger;

        public NotificationServices(AppDbContext context, ILogger<NotificationServices> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<NotificationDto> GetUserNotifications(int userId)
        {
            return _context.Notification
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    BookTitle = n.BookTitle,
                    Message = n.Message,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead,
                    Type = n.Type
                })
                .ToList();
        }

        public bool MarkAsRead(int notificationId, int userId)
        {
            var notification = _context.Notification
                .FirstOrDefault(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            _context.SaveChanges();
            return true;
        }

        public int GetUnreadCount(int userId)
        {
            return _context.Notification
                .Count(n => n.UserId == userId && !n.IsRead);
        }

        public void CreateNotification(NotificationDto notificationDto)
        {
            var notification = new Notification
            {
                UserId = notificationDto.UserId,
                BookTitle = notificationDto.BookTitle,
                Message = notificationDto.Message,
                Type = notificationDto.Type,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notification.Add(notification);
            _context.SaveChanges();
        }
    }
}

