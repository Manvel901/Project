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
        public ActionResult<decimal> GetPenalty(int reservationId)
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
        [HttpPost("return")]
        public IActionResult ReturnBook([FromRoute] int bookId)
        {
            try
            {
                _reservPenalty.ReturnBook(bookId);
                return Ok();
            }
            catch (KeyNotFoundException e) { return NotFound(e.Message); }
            catch (InvalidOperationException e) { return BadRequest(e.Message); }
        }

        [HttpGet("by-reservation/{reservationId}")]
        public IActionResult GetByReservation(int reservationId)
        {
            return Ok(_reservPenalty.GetPenaltiesByUserReservation(reservationId));
        }

    }
}
