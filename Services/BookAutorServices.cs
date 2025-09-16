using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static Diplom.Models.ApDbContext;

namespace Diplom.Services
{
    public class BookAutorServices : IBookAutor
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mupper;
        private readonly IMemoryCache _cache;

        public BookAutorServices(AppDbContext context, IMapper mapper, IMemoryCache cache )
        {
            _context = context;
            _mupper = mapper;
            _cache = cache;
        }
        public void AddAuthorToBook(BookDto bookDto,  AutorDto autorDto)
        {
            using (_context)
            {
                var book = _context.Books.Find(bookDto.Id);
                var author = _context.Authors.Find(autorDto.Id);

                if (book == null || author == null)
                    throw new KeyNotFoundException("Книга или автор не найдены.");
                if (_context.Books
                    .Include(b => b.Authors)
                    .FirstOrDefault(b => b.Id == bookDto.Id)?
                    .Authors.Any(a => a.Id == autorDto.Id) == true)
                {
                    throw new InvalidOperationException("Автор уже связан с этой книгой.");
                }

                book.Authors.Add(new Autors { Id = autorDto.Id });
                
                _context.SaveChanges();
                _cache.Remove("autors");
            }
        }

        public AutorDto CreateAuthor(string firstname, string surname, string lastname, string bio)
        {
            if (_context.Authors.Any(a => a.FirstName == firstname && a.SurName == surname && a.LastName == lastname))
                throw new ArgumentException("Автор с таким именем уже существует.");

            var author = new Autors { FirstName = firstname, SurName = surname, LastName= lastname, Bio = bio };
            _context.Authors.Add(author);
            _context.SaveChanges();
            return author;
        }

        public void DeleteAuthor(int authorId)
        {
            var author = _context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefault(a => a.Id == authorId);

            if (author == null) return;

            if (author.BookAuthors.Any())
                throw new InvalidOperationException("Нельзя удалить автора с привязанными книгами.");

            _context.Authors.Remove(author);
            _context.SaveChanges();
        }

        public IEnumerable<AutorDto> GetAllAuthors()
        {
            return _context.Authors.ToList();
        }

        public AutorDto GetAuthorById(int authorId)
        {
            return _context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefault(a => a.Id == authorId);
        }

        public IEnumerable<AutorDto> GetAuthorsByBook(int bookId)
        {
            return _context.BookAuthors
            .Where(ba => ba.BookId == bookId)
            .Select(ba => ba.Autor)
            .ToList();
        }

        public IEnumerable<BookDto> GetBooksByAuthor(int authorId)
        {
            return _context.BookAuthors
            .Where(ba => ba.AuthorId == authorId)
            .Select(ba => ba.Book)
            .ToList();
        }

        public void RemoveAuthorFromBook(int bookId, int authorId)
        {
            var link = _context.BookAuthors
            .FirstOrDefault(ba => ba.BookId == bookId && ba.AuthorId == authorId);

            if (link == null) return;

            _context.BookAuthors.Remove(link);
            _context.SaveChanges();
        }

        public AutorDto UpdateAuthor(int authorId, string newFirstName, string newSurName, string newLastName, string newBio)
        {
            var author = _context.Authors.Find(authorId);
            if (author == null)
                throw new KeyNotFoundException("Автор не найден.");

            author.FirstName = newFirstName;
            author.SurName = newSurName;
            author.LastName = newLastName;
            author.Bio = newBio;
            _context.SaveChanges();
            return author;
        }
    }
}
