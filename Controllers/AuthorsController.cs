using Diplom.Abstract;
using Diplom.Models.dto;
using Diplom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorsService _authorsService;
        private readonly ILogger _logger;
      

        public AuthorsController(IAuthorsService authorsService, ILogger<AuthorsController> logger)
        {
            _authorsService = authorsService;
            _logger = logger;
        }
        // GET: /Authors
        [AllowAnonymous]
        [HttpGet("GetAll")]
        public IActionResult GetAllAuthors()
        {
            var authors = _authorsService.GetAllAuthors();
            return Ok(authors);
        }

        // GET: /Authors/5
        [AllowAnonymous]
        [HttpGet("GetFromId")]
        public IActionResult GetAuthor(int id)
        {
            var author = _authorsService.GetAuthorById(id);
            return author == null ? NotFound() : Ok(author);
        }

        // POST: /Authors
       
        [HttpPost("Create")]
        public IActionResult CreateAuthor([FromBody] AutorDto autorDto)
        {
            
            try
            {
                var createdId = _authorsService.CreateAuthor(autorDto);
                return Ok(createdId);
            }
           
                // При необходимости можно уточнить исключения в сервисе и вернуть более точные коды
                catch (DbUpdateException ex)
                {
                var details = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Save failed: {Details}", details);
                throw;
                }
        }
        

        // PUT: /Authors/5
        [Authorize(Roles = "Admin")]
        [HttpPut("Update")]
        public IActionResult UpdateAuthor( [FromBody] AutorDto autorDto)
        {
            

            try
            {
                var updatedId = _authorsService.UpdateAuthor(autorDto);
                return Ok(updatedId);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE: /Authors/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete")]
        public IActionResult DeleteAuthor(int id)
        {
            try
            {
                _authorsService.DeleteAuthor(id);
                return NoContent();
            }
            catch (InvalidOperationException e) // например, есть связанные книги
            {
                return Conflict(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
