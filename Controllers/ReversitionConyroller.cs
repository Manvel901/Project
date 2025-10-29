using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")] // Base route: /reservations
    public class ReservationController : ControllerBase
    {
        private readonly IReservation _reservationService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(IReservation reservationService, ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        // Create reservation (requires authorization)
        [Authorize]
        [HttpPost("CreateReservationByTitle")]
        public async Task<IActionResult> CreateReservationByTitle([FromBody] ReservationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var reservation = await _reservationService.ReserveBookByTitleAndAuthor(request.BookTitle, request.AuthorName, User);
                return CreatedAtAction(nameof(GetReservation), new { id = reservation.BookId }, reservation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Book not found while creating reservation: {Title}, {Author}", request.BookTitle, request.AuthorName);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating reservation: {Title}, {Author}", request.BookTitle, request.AuthorName);
                return BadRequest(ex.Message);
            }
        }

        // Cancel reservation (only owner or admin)
        [Authorize]
        [HttpDelete("DeleteReservation/{id}")]
        public IActionResult CancelReservation(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var reservation = _reservationService.GetReservationById(id);

                // Check permissions: user can only cancel their own reservation
                if (reservation == null || (reservation.UserId != userId && !User.IsInRole("Admin")))
                    return Forbid();

                _reservationService.CancelReservation(id);
                _logger.LogInformation("Reservation {ReservationId} canceled", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Attempt to cancel non-existent reservation {ReservationId}", id);
                return NotFound(ex.Message);
            }
        }

        // Get reservation by ID (owner or admin)
        [Authorize]
        [HttpGet("GetById/{id}")]
        public IActionResult GetReservation(int id)
        {
            var reservation = _reservationService.GetReservationById(id);
            if (reservation == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(reservation);
        }

        // Get all reservations for the current user
        [Authorize]
        [HttpGet("myReservations")]
        public IActionResult GetMyReservations()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var reservations = _reservationService.GetUserReservations(userId);
            return Ok(reservations);
        }

        // Get all overdue reservations (admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("overdue")]
        public IActionResult GetOverdueReservations()
        {
            var reservations = _reservationService.GetOverdueReservations();
            return Ok(reservations);
        }

        // Update reservation status (admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateReservation")]
        public IActionResult UpdateStatus([FromBody] ReservationDto reservationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

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

        // Reservation request DTO
        public class ReservationRequest
        {
            [Required]
            public string BookTitle { get; set; }

            [Required]
            public string AuthorName { get; set; }
        }
    }
}
