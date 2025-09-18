using Diplom.Abstract;
using Diplom.Models.dto;
using Diplom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorsService _authorsService;
        public AuthorsController() { }

        public AuthorsController(IAuthorsService authorsService)
        {
            _authorsService = authorsService;
        }
        // GET: /Authors
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllAuthors()
        {
            var authors = _authorsService.GetAllAuthors();
            return Ok(authors);
        }

        // GET: /Authors/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetAuthor(int id)
        {
            var author = _authorsService.GetAuthorById(id);
            return author == null ? NotFound() : Ok(author);
        }

        // POST: /Authors
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AutorDto autorDto)
        {
            if (autorDto == null) return BadRequest();
            try
            {
                var createdId = _authorsService.CreateAuthor(autorDto);
                return Ok(createdId);
            }
            catch (Exception e)
            {
                // При необходимости можно уточнить исключения в сервисе и вернуть более точные коды
                return BadRequest(e.Message);
            }
        }

        // PUT: /Authors/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateAuthor(int id, [FromBody] AutorDto autorDto)
        {
            if (autorDto == null || id != autorDto.Id) return BadRequest();

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
        [HttpDelete("{id}")]
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
