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
            [HttpGet("GetBookById")]
            public IActionResult GetBook(int id)
            {
                var book = _bookService.GetBookById(id);
                return book == null ? NotFound() : Ok(book);
            }

            // POST: api/books
            [Authorize(Roles = "Admin")]
            [HttpPost("CreateBook")]
            public IActionResult AddBook([FromBody] BookDto bookDto)
            {
                var createdBook = _bookService.AddBook(bookDto);
                return Ok(createdBook);
            }

            // PUT: api/books/5
            [Authorize(Roles = "Admin")]
            [HttpPut("UpdateBook")]
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
            [HttpDelete("DeleteBook")]
            public IActionResult DeleteBook(int id)
            {
                _bookService.DeleteBook(id);
                return NoContent();
            }

            // GET: api/books/search?query=...
            [AllowAnonymous]
            [HttpGet("SearchBook")]
            public IActionResult SearchBooks([FromQuery] string query)
            {
                return Ok(_bookService.SearchBooksByTitle(query));
            }
        
       

       
        public class ReserveRequestDto
        {
            public BookDto Book { get; set; }
            public UserDto User { get; set; }
        }
    }
    
}
