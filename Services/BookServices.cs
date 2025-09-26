using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static Diplom.Models.AppDbContext;

namespace Diplom.Services
{
    public class BookServices : IBookServicrs
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public BookServices(AppDbContext context, IMapper mapper, IMemoryCache cache )
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        public int AddBook(BookDto bookDto)
        {
            using (_context)
            {


                var entity = _context.Books.FirstOrDefault(x => x.ISBN.ToLower() == bookDto.ISBN.ToLower());

                if (entity == null)
                {
                    entity = _mapper.Map<Book>(bookDto);
                    _context.Books.Add(entity);
                    _context.SaveChanges();
                    _cache.Remove("books");
                }
                

               
                return entity.Id;
            }
        }

        public void DeleteBook(int bookId)
        {
            using (_context)
            {
                
              var book = _context.Books.Find(bookId);
              if (book == null) return;

              _context.Books.Remove(book);
               _context.SaveChanges();
                _cache.Remove("books");
            }
        }

        public IEnumerable<BookDto> GetAllBooks()
        {
            using (_context)
            {
                if (_cache.TryGetValue("books", out List<BookDto> booksDto))
                {
                    return booksDto;
                }
                var list = _context.Books.Select(x => _mapper.Map<BookDto>(x)).ToList();
                return list;
            }
        }

        public BookDto GetBookById(int bookId)
        {
            using (_context)
            {
               var book =  _context.Books
                 .Include(b => b.Reservations)
                 .FirstOrDefault(b => b.Id == bookId);

                return _mapper.Map<BookDto>(book);
            }
        }

        public IEnumerable<ReservationDto> GetOverdueReservations(int bookId)
        {
            using (_context)
            {

             var list =  _context.Reserv
           .Where(r => r.BookId == bookId && r.DueDate < DateTime.Now && r.Status == "Active");

            return  list.Select(x=> _mapper.Map<ReservationDto>(x)).ToList();
            }
            
        }



        public void ReserveBook(BookDto bookDto, UserDto userDto)
        {
            // не disopose контекст из DI
            var book = _context.Books.Find(bookDto.Id);
            if (book == null) throw new Exception("Книга не найдена.");
            if (book.AvailableCopies < 1) throw new InvalidOperationException("Нет доступных экземпляров.");

            var nowUtc = DateTime.UtcNow;

            var reservation = new ReservationDto
            {
                BookId = bookDto.Id,
                UserId = userDto.Id,
                ReservationDate = nowUtc,
                DueDate = nowUtc.AddDays(14),
                Status = "Active"
            };

            book.AvailableCopies--;

            var res = _mapper.Map<Reservation>(reservation);

            // Если AutoMapper не устанавливает Kind, убедитесь, что в сущности даты тоже имеют Kind = Utc:
            if (res.ReservationDate.Kind != DateTimeKind.Utc)
                res.ReservationDate = DateTime.SpecifyKind(res.ReservationDate, DateTimeKind.Utc);
            if (res.DueDate.Kind != DateTimeKind.Utc)
                res.DueDate = DateTime.SpecifyKind(res.DueDate, DateTimeKind.Utc);

            _context.Reserv.Add(res);
            _context.SaveChanges();
        }

        public void ReturnBook(int bookId)
        {
            using (_context)
            {
                var book = _context.Books.Find(bookId);
                if (book == null) throw new KeyNotFoundException("Книга не найдена.");

                book.AvailableCopies++;
                _context.SaveChanges();
            }
        }

        

        public IEnumerable<BookDto> SearchBooksByTitle(string bookTitle)
        {
            using (_context)
            {
                var list = _context.Books
               .Where(b => b.BookTitle.Contains(bookTitle));

                return list.Select(x => _mapper.Map<BookDto>(x)).ToList();
            }
           
        }

        public int UpdateBook( BookDto bookDto)
        {
            using (_context)
            {

                var book = _context.Books.Find(bookDto.Id);
                if (book == null) throw new KeyNotFoundException("Книга не найдена.");

                // Обновление доступных экземпляров
                int copiesDifference = bookDto.TotalCopies - book.TotalCopies;
                book.AvailableCopies += copiesDifference;

                // Обновление полей
                book.BookTitle = bookDto.BookTitle;

                book.GenreId = bookDto.GenreId;
                book.TotalCopies = bookDto.TotalCopies;

                _context.SaveChanges();
                return book.Id;
            }
        }

       
       

    }
}

