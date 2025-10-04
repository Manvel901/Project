using Autofac.Core;
using Diplom.Abstract;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    public class PenaltyesController:ControllerBase
    {
        private readonly IPenaltyService _penaltyService;
      
        public PenaltyesController(IPenaltyService penaltyService)
        {
            _penaltyService = penaltyService;
        }
        [HttpPost("CreatePenalty")]
        public IActionResult Create([FromBody]  PenaltyDto penaltyDto, [FromQuery] int reservId)
        {
            var id = _penaltyService.CreatePenalty(penaltyDto, reservId);
            return CreatedAtAction(nameof(GetById), new { penaltyId = id }, id);
        }

        [HttpPost("PayPenalty")]
        public IActionResult Pay( [FromQuery] int id, int amountPaid, DateTime? paidAtUtc)
        {
            var ok = _penaltyService.PayPenalty(id, amountPaid, paidAtUtc);
            if (!ok) return BadRequest();
            return NoContent();
        }

        [HttpGet("GetPenaltyById")]
        public IActionResult GetById(int penaltyId)
        {
            var p = _penaltyService.GetPenaltyById(penaltyId);
            if (p == null) return NotFound();
            return Ok(p);
        }

        
    }
}
