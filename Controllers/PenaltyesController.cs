using Autofac.Core;
using Diplom.Abstract;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    public class PenaltyesController:ControllerBase
    {
        private readonly IPenaltyService _penaltyService;
        public PenaltyesController()
        {
            
        }
        public PenaltyesController(IPenaltyService penaltyService)
        {
            _penaltyService = penaltyService;
        }
        [HttpPost]
        public IActionResult Create([FromBody]  PenaltyDto penaltyDto)
        {
            var id = _penaltyService.CreatePenalty(penaltyDto);
            return CreatedAtAction(nameof(GetById), new { penaltyId = id }, id);
        }

        [HttpPost("{penaltyId}/pay")]
        public IActionResult Pay( [FromBody] PenaltyDto penaltyDto)
        {
            var ok = _penaltyService.PayPenalty(penaltyDto);
            if (!ok) return BadRequest();
            return NoContent();
        }

        [HttpGet("{penaltyId}")]
        public IActionResult GetById(int penaltyId)
        {
            var p = _penaltyService.GetPenaltyById(penaltyId);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpGet("by-reservation/{reservationId}")]
        public IActionResult GetByReservation(int reservationId)
        {
            return Ok(_penaltyService.GetPenaltiesByUserReservation(reservationId));
        }
    }
}
