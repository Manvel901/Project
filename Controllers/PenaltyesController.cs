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
                _logger.LogError(ex, "Ошибка при получении штрафов пользователя");
                return StatusCode(500, "Внутренняя ошибка сервера");
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
    }
}