using Autofac.Core;
using Diplom.Abstract;
using Diplom.Services;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservPenaltyController:ControllerBase
    {
        private readonly IReservPenalty _reservPenalty;
        public ReservPenaltyController(IReservPenalty reservPenalty)
        {
            _reservPenalty = reservPenalty;
        }


        [HttpGet("Getpenalty")]
        public IActionResult GetPenalty(int reservationId)
        {
            try
            {
                var amount = _reservPenalty.CalculatePenalty(reservationId);
                return Ok(amount);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }
        // POST /Books/return/{bookId}
        [HttpPost("returnBook")]
        public IActionResult ReturnBook( int reservationId)
        {
            try
            {
                _reservPenalty.ReturnBook(reservationId);
                return Ok();
            }
            catch (KeyNotFoundException e) { return NotFound(e.Message); }
            catch (InvalidOperationException e) { return BadRequest(e.Message); }
        }

        [HttpGet("GetPenalByReservation")]
        public IActionResult GetByReservation(int reservationId)
        {
            return Ok(_reservPenalty.GetPenaltiesByUserReservation(reservationId));
        }

    }
}
