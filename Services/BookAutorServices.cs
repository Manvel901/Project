using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static Diplom.Models.AppDbContext;

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
        public int AddAuthorToBook(int bookId, int authorId)
        {
            using (_context)
            {
                // не диспоузим _context, если он из DI
                var book = _context.Books
                    .Include(b => b.Authors)
                    .FirstOrDefault(b => b.Id == bookId);

                if (book == null) throw new KeyNotFoundException("Книга не найдена.");

                // проверяем существование автора в БД
                var author = _context.Authors.Find(authorId);
                if (author == null) throw new KeyNotFoundException("Автор не найден.");

                // проверяем, связан ли автор уже с книгой
                if (book.Authors.Any(a => a.Id == authorId))
                    throw new InvalidOperationException("Автор уже связан с этой книгой.");

                // используем найденную отслеживаемую сущность
                book.Authors.Add(author);

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
                    .Where(b => b.Id == bookId)
                    .SelectMany(b => b.Authors)          // раскрыть коллекцию
                    .Select(a => _mupper.Map<AutorDto>(a))
                    .ToList();
            }

        }

            
        

        public IEnumerable<BookDto> GetBooksByAuthor(int authorId)
        {
            using (_context)
            {
                return _context.Authors
                .Where(ba => ba.Id == authorId)
                .SelectMany(ba => ba.Books).Select(a=> _mupper.Map<BookDto>(a)).ToList();

                
                
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
