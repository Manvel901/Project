using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
  
        [ApiController]
        [Route("[controller]")]
        public class BookController : ControllerBase
        {
            private readonly IBookServicrs _bookService;

            public BookController(IBookServicrs bookService)
            {
                _bookService = bookService;
            }

            // GET: api/books
            [AllowAnonymous]
            [HttpGet]
            public IActionResult GetAllBooks()
            {
                return Ok(_bookService.GetAllBooks());
            }

            // GET: api/books/5
            [AllowAnonymous]
            [HttpGet("{id}")]
            public IActionResult GetBook(int id)
            {
                var book = _bookService.GetBookById(id);
                return book == null ? NotFound() : Ok(book);
            }

            // POST: api/books
            [Authorize(Roles = "Admin")]
            [HttpPost]
            public IActionResult AddBook([FromBody] BookDto bookDto)
            {
                var createdBook = _bookService.AddBook(bookDto);
                return Ok(createdBook);
            }

            // PUT: api/books/5
            [Authorize(Roles = "Admin")]
            [HttpPut("{id}")]
            public IActionResult UpdateBook([FromBody] BookDto bookdto)
            {

            try
            {
                var updatedBook = _bookService.UpdateBook(bookdto);
                return Ok(updatedBook);

            }
            catch (KeyNotFoundException ex)
               {
                return NotFound(ex.Message);
                }
            }

            // DELETE: api/books/5
            [Authorize(Roles = "Admin")]
            [HttpDelete("{id}")]
            public IActionResult DeleteBook(int id)
            {
                _bookService.DeleteBook(id);
                return NoContent();
            }

            // GET: api/books/search?query=...
            [AllowAnonymous]
            [HttpGet("search")]
            public IActionResult SearchBooks([FromQuery] string query)
            {
                return Ok(_bookService.SearchBooksByTitle(query));
            }
        [HttpPost("reserve")]
        public IActionResult ReserveBook([FromQuery] BookDto bookDto, [FromBody] UserDto userDto  )
        {
            if (bookDto == null || userDto == null) return BadRequest();

            try
            {
                _bookService.ReserveBook(bookDto ,userDto);
                return Ok();
            }
            catch (KeyNotFoundException e) { return NotFound(e.Message); }
            catch (InvalidOperationException e) { return Conflict(e.Message); }
        }

        // POST /Books/return/{bookId}
        [HttpPost("return")]
        public IActionResult ReturnBook([FromRoute] int bookId)
        {
            try
            {
                _bookService.ReturnBook(bookId);
                return Ok();
            }
            catch (KeyNotFoundException e) { return NotFound(e.Message); }
            catch (InvalidOperationException e) { return BadRequest(e.Message); }
        }
        public class ReserveRequestDto
        {
            public BookDto Book { get; set; }
            public UserDto User { get; set; }
        }
    }
    
}
