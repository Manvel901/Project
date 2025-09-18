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
        public int AddAuthorToBook(BookDto bookDto,  AutorDto autorDto)
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
                return book.Id;
            }
        }

       
        public IEnumerable<AutorDto> GetAuthorsByBook(int bookId)
        {
            using (_context)
            {
                return _context.Books
                .Where(ba => ba.Id == bookId)
                .Select(ba => _mupper.Map<AutorDto>(ba.Authors))
                .ToList();
            }

            
        }

        public IEnumerable<BookDto> GetBooksByAuthor(int authorId)
        {
            using (_context)
            {
                return _context.Authors
                .Where(ba => ba.Id == authorId)
                .Select(ba => _mupper.Map<BookDto>(ba.Books))
                .ToList();
            }
        }

        public int RemoveAuthorFromBook(int bookId, int authorId)
        {
            using (_context)
            {
                var link = _context.Books.Include(a => a.Authors).
                    FirstOrDefault(b => b.Id == bookId && b.Authors.Any(a => a.Id == authorId));


                if (link != null)
                {

                    _context.Books.Remove(link);
                    _context.SaveChanges();
                    _cache.Remove("authors");
                }
                return link.Id;
            }

        }

       
    }
}
