using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")] // Базовый маршрут: /reservations
    public class ReservationController : ControllerBase
    {
        private readonly IReservation _reservationService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IReservation reservationService,
            ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        // Создать бронирование (требуется авторизация)
        [Authorize]
        [HttpPost("CreateReservation")]
        public IActionResult CreateReservation([FromQuery] int userId, int bookId, string bookTitle)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Некорректные данные");

               
                var reservation = _reservationService.CreateReservation(userId, bookId, bookTitle);

                _logger.LogInformation("Создано бронирование {ReservationId} для пользователя {UserId}", reservation.Id, userId);
                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Ошибка создания бронирования: книга или пользователь не найдены");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Ошибка создания бронирования: книга недоступна");
                return BadRequest(ex.Message);
            }
        }

        // Отменить бронирование (только владелец или администратор)
        [Authorize]
        [HttpDelete("DeleteReservation")]
        public IActionResult CancelReservation(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var reservation = _reservationService.GetReservationById(id);

                // Проверка прав: пользователь может отменить только своё бронирование
                if (reservation.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                _reservationService.CancelReservation(id);
                _logger.LogInformation("Бронирование {ReservationId} отменено", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Попытка отмены несуществующего бронирования {ReservationId}", id);
                return NotFound(ex.Message);
            }
        }

        // Получить бронирование по ID (владелец или администратор)
        [Authorize]
        [HttpGet("GetById")]
        public IActionResult GetReservation(int id)
        {
            var reservation = _reservationService.GetReservationById(id);
            if (reservation == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(reservation);
        }

        // Получить все бронирования текущего пользователя
        [Authorize]
        [HttpGet("myRservetion")]
        public IActionResult GetMyReservations(int userId)
        {
            
            var reservations = _reservationService.GetUserReservations(userId);
            return Ok(reservations);
        }

        // Получить все просроченные бронирования (только администратор)
        [Authorize(Roles = "Admin")]
        [HttpGet("overdue")]
        public IActionResult GetOverdueReservations()
        {
            var reservations = _reservationService.GetOverdueReservations();
            return Ok(reservations);
        }

        // Обновить статус бронирования (администратор)
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateReservation")]
        public IActionResult UpdateStatus( [FromBody] ReservationDto reservationDto)
        {
            try
            {
                _reservationService.UpdateReservationStatus(reservationDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        public class ReservationCreateRequest
        {
            [Required(ErrorMessage = "Идентификатор книги обязателен")]
            [Range(1, int.MaxValue, ErrorMessage = "Некорректный BookId")]
            public int BookId { get; set; }
        }
    }
}
