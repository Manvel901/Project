using Autofac.Core;
using Diplom.Abstract;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookAutorController:ControllerBase
    {
        private readonly IBookAutor _bookAutor;
        public BookAutorController()
        {
            
        }
        public BookAutorController(IBookAutor bookAutor)
        {
            _bookAutor = bookAutor;
        }

        // POST: /BookAuthor
        // Добавить автора к книге
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddAuthorToBook(BookDto bookDto, AutorDto autorDto)
        {
            
            try
            {
                var bookId = _bookAutor.AddAuthorToBook(bookDto, autorDto);
                return Ok(bookId);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return Conflict(e.Message);
            }
        }

        // GET: /BookAuthor/book/5/authors
        [AllowAnonymous]
        [HttpGet("book/{bookId}/authors")]
        public IActionResult GetAuthorsByBook(int bookId)
        {
            var authors = _bookAutor.GetAuthorsByBook(bookId);
            if (authors == null) return NotFound();
            return Ok(authors);
        }

        // GET: /BookAuthor/author/5/books
        [AllowAnonymous]
        [HttpGet("author/{authorId}/books")]
        public IActionResult GetBooksByAuthor(int authorId)
        {
            var books = _bookAutor.GetBooksByAuthor(authorId);
            if (books == null) return NotFound();
            return Ok(books);
        }

        // DELETE: /BookAuthor/book/5/author/3
        [Authorize(Roles = "Admin")]
        [HttpDelete("book/{bookId}/author/{authorId}")]
        public IActionResult RemoveAuthorFromBook(int bookId, int authorId)
        {
            try
            {
                var removedBookId = _bookAutor.RemoveAuthorFromBook(bookId, authorId);
                return NoContent();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }



}

