using Diplom.Abstract;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenresController:ControllerBase
    {
        private readonly IGenresServices _genresServices;
        public GenresController(IGenresServices genresServices)
        {
            _genresServices = genresServices;
        }
        [HttpPost]
        public IActionResult Create([FromBody] GenresDto genreDto)
        {
            if (genreDto == null) return BadRequest();

            var id = _genresServices.CreateGenre(genreDto);

            return Ok( id);
        }

        // PUT /Genres/{id}
        [HttpPut("Update")]
        public IActionResult Update(int id, [FromBody] GenresDto genreDto)
        {
            if (genreDto == null) return BadRequest();
            if (id != genreDto.Id) return BadRequest("Id в пути и в теле не совпадают");

            var updatedId = _genresServices.UpdateGenre(genreDto);
            if (updatedId == 0) return NotFound();

            return Ok(genreDto);
        }

        // DELETE /Genres/{id}
        [HttpDelete("Remove")]
        public IActionResult Delete(int id)
        {
            try
            {
                _genresServices.DeleteGenre(id);
                return NoContent();
            }
            catch (InvalidOperationException ex) // например жанр используется в книгах
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET /Genres/{id}
        [HttpGet("ById")]
        public IActionResult GetById(int id)
        {
            var genre = _genresServices.GetGenreById(id);
            if (genre == null) return NotFound();
            return Ok(genre);
        }

        // GET /Genres
        [HttpGet("All")]
        public IActionResult GetAll()
        {
            var list = _genresServices.GetAllGenres();
            return Ok(list);
        }
    }
}
