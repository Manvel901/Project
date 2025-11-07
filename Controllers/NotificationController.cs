using Diplom.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Diplom.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationServices _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationServices notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("MyNotifications")]
        public IActionResult GetMyNotifications()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notifications = _notificationService.GetUserNotifications(userId);

                _logger.LogInformation("Пользователь {UserId} запросил уведомления", userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении уведомлений");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [Authorize]
        [HttpPost("MarkAsRead/{notificationId}")]
        public IActionResult MarkAsRead(int notificationId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var success = _notificationService.MarkAsRead(notificationId, userId);

                if (success)
                {
                    _logger.LogInformation("Пользователь {UserId} отметил уведомление {NotificationId} как прочитанное",
                        userId, notificationId);
                    return Ok(new { Message = "Уведомление отмечено как прочитанное" });
                }
                else
                {
                    return NotFound("Уведомление не найдено");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отметке уведомления как прочитанного");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [Authorize]
        [HttpGet("UnreadCount")]
        public IActionResult GetUnreadCount()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var count = _notificationService.GetUnreadCount(userId);

                return Ok(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества непрочитанных уведомлений");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [Authorize]
        [HttpPost("MarkAllAsRead")]
        public IActionResult MarkAllAsRead()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notifications = _notificationService.GetUserNotifications(userId)
                    .Where(n => !n.IsRead)
                    .ToList();

                foreach (var notification in notifications)
                {
                    _notificationService.MarkAsRead(notification.Id, userId);
                }

                _logger.LogInformation("Пользователь {UserId} отметил все уведомления как прочитанные", userId);
                return Ok(new { Message = "Все уведомления отмечены как прочитанные", Count = notifications.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отметке всех уведомлений как прочитанных");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}
