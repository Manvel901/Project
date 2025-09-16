using Diplom.Abstract;
using Diplom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("top-books")]
        public ActionResult<List<BookStatistic>> GetTopBooks(
            [FromQuery] int top = 10,
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null)
        {
            return Ok(_analyticsService.GetTopBooks(top, start, end));
        }
    }
}
