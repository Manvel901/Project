using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface INotificationServices
    {
        IEnumerable<NotificationDto> GetUserNotifications(int userId);
        bool MarkAsRead(int notificationId, int userId);
        int GetUnreadCount(int userId);
        void CreateNotification(NotificationDto notificationDto);
    }
}
