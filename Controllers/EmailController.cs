using Diplom.Abstract;

namespace Diplom.Controllers
{
    using global::Diplom.Models.dto;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    namespace Diplom.Controllers
    {
        [ApiController]
        [Route("[controller]")]
        public class EmailController : ControllerBase
        {
            private readonly IEmailService _emailService;
            private readonly ILogger<EmailController> _logger;

            public EmailController(IEmailService emailService, ILogger<EmailController> logger)
            {
                _emailService = emailService;
                _logger = logger;
            }

            [Authorize]
            [HttpPost("CreateComment")]
            public IActionResult CreateComment([FromBody] EmailDto emailDto)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                try
                {
                    // Получаем ID пользователя из токена
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!int.TryParse(userIdClaim, out var userId))
                    {
                        return Unauthorized("Неверный идентификатор пользователя");
                    }

                    // Устанавливаем UserId из авторизации
                    emailDto.UserId = userId;

                    var id = _emailService.CreateComment(emailDto);
                    _logger.LogInformation("Пользователь {UserId} создал комментарий с id {CommentId}", userId, id);
                    return Ok(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании комментария");
                    return StatusCode(500, "Внутренняя ошибка сервера");
                }
            }

            [AllowAnonymous]
            [HttpGet("GetAllComments")]
            public IActionResult GetAllComments()
            {
                try
                {
                    var comments = _emailService.GetAll();
                    return Ok(comments);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении комментариев");
                    return StatusCode(500, "Внутренняя ошибка сервера");
                }
            }

            [Authorize]
            [HttpGet("MyComments")]
            public IActionResult GetMyComments()
            {
                try
                {
                    // Получаем ID пользователя из токена
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!int.TryParse(userIdClaim, out var userId))
                    {
                        return Unauthorized("Неверный идентификатор пользователя");
                    }

                    // Получаем все комментарии и фильтруем по текущему пользователю
                    var allComments = _emailService.GetAll();
                    var myComments = allComments.Where(c => c.UserId == userId).ToList();

                    return Ok(myComments);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении комментариев пользователя");
                    return StatusCode(500, "Внутренняя ошибка сервера");
                }
            }
        }
    }
}
