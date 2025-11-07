using Autofac.Core;
using Diplom.Abstract;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PenaltyController : ControllerBase
    {
        private readonly IPenaltyService _penaltyService;
        private readonly ILogger<PenaltyController> _logger;

        public PenaltyController(IPenaltyService penaltyService, ILogger<PenaltyController> logger)
        {
            _penaltyService = penaltyService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("CreatePenalty")]
        public IActionResult Create([FromBody] PenaltyDto penaltyDto, [FromQuery] int reservId)
        {
            var id = _penaltyService.CreatePenalty(penaltyDto, reservId);
            return CreatedAtAction(nameof(GetById), new { penaltyId = id }, id);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateOverduePenalty")]
        public IActionResult CreateOverduePenalty([FromQuery] int reservationId)
        {
            try
            {
                var id = _penaltyService.CreateOverduePenalty(reservationId);
                return Ok(new { PenaltyId = id, Message = "Overdue penalty created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating overdue penalty for reservation {ReservationId}", reservationId);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CheckOverduePenalties")]
        public IActionResult CheckOverduePenalties()
        {
            try
            {
                var createdCount = _penaltyService.CheckAndCreateOverduePenalties();
                return Ok(new { CreatedCount = createdCount, Message = $"Created {createdCount} overdue penalties" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overdue penalties");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpGet("MyPenalties")]
        public IActionResult GetUserPenalties()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var penalties = _penaltyService.GetUserPenalties(userId);
                return Ok(penalties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user penalties");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpGet("ByReservation/{reservationId}")]
        public IActionResult GetByReservation(int reservationId)
        {
            try
            {
                var penalties = _penaltyService.GetPenaltiesByReservation(reservationId);
                return Ok(penalties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении штрафов по бронированию {ReservationId}", reservationId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [Authorize]
        [HttpGet("{penaltyId}")]
        public IActionResult GetById(int penaltyId)
        {
            try
            {
                var penalty = _penaltyService.GetPenaltyById(penaltyId);
                return penalty == null ? NotFound() : Ok(penalty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении штрафа {PenaltyId}", penaltyId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize]
        [HttpPost("PayPenalty")]
        public IActionResult PayPenalty([FromBody] PayPenaltyRequest request)
        {
            try
            {
                var success = _penaltyService.PayPenalty(request.PenaltyId, request.AmountPaid, request.PaidAtUtc);

                if (success)
                {
                    _logger.LogInformation("Пользователь оплатил штраф {PenaltyId} на сумму {Amount}",
                        request.PenaltyId, request.AmountPaid);
                    return Ok(new { Message = "Штраф успешно оплачен" });
                }
                else
                {
                    return BadRequest("Не удалось оплатить штраф. Проверьте ID штрафа и сумму.");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные данные при оплате штрафа {PenaltyId}", request.PenaltyId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оплате штрафа {PenaltyId}", request.PenaltyId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        public class PayPenaltyRequest
        {
            public int PenaltyId { get; set; }
            public decimal AmountPaid { get; set; }
            public DateTime? PaidAtUtc { get; set; }
        }
    }
}